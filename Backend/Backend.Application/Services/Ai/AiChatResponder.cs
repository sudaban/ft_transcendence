using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Responses.ChatRooms;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.Services.Ai;

public class AiChatResponder : IAiChatResponder
{
    private const int MaxRequestsPerWindow = 5;
    private const int HistoryMessageCount = 20;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    private static readonly ConcurrentDictionary<int, List<DateTime>> _usage = new();

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AiChatResponder> _logger;

    public AiChatResponder(IServiceScopeFactory scopeFactory, ILogger<AiChatResponder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void QueueReply(int roomId, int aiUserId, int requestingUserId)
    {
        _ = Task.Run(() => RespondAsync(roomId, aiUserId, requestingUserId));
    }

    private enum RateState { Allowed, JustExceeded, Silenced }

    private static RateState CheckRateLimit(int userId)
    {
        var now = DateTime.UtcNow;
        var timestamps = _usage.GetOrAdd(userId, _ => new List<DateTime>());
        lock (timestamps)
        {
            timestamps.RemoveAll(t => now - t > Window);
            timestamps.Add(now);
            if (timestamps.Count <= MaxRequestsPerWindow)
                return RateState.Allowed;
            if (timestamps.Count == MaxRequestsPerWindow + 1)
                return RateState.JustExceeded;
            return RateState.Silenced;
        }
    }

    private async Task RespondAsync(int roomId, int aiUserId, int requestingUserId)
    {
        using var scope = _scopeFactory.CreateScope();
        var chatHubService = scope.ServiceProvider.GetRequiredService<IChatHubService>();

        var rateState = CheckRateLimit(requestingUserId);
        if (rateState == RateState.Silenced)
            return;

        try
        {
            await chatHubService.SendAiTypingAsync(roomId, true);

            var aiService = scope.ServiceProvider.GetRequiredService<IAiService>();
            var messageRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<Message>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            string replyContent;

            if (rateState == RateState.JustExceeded)
            {
                replyContent = "🚦 Slow down a bit! You can send me at most " + MaxRequestsPerWindow +
                               " messages per minute.";
            }
            else if (!aiService.IsConfigured)
            {
                replyContent = "⚠️ I am not fully set up yet: GEMINI_API_KEY is missing. " +
                               "See HOW_TO_GET_AI_KEY.md in the project root to enable me.";
            }
            else
            {
                var recentMessages = await messageRepository.TableNoTracking
                    .Where(m => m.ChatRoomId == roomId)
                    .OrderByDescending(m => m.SentAt)
                    .Take(HistoryMessageCount)
                    .ToListAsync();

                var history = recentMessages
                    .OrderBy(m => m.SentAt)
                    .Select(m => new AiChatMessage(m.SenderId == aiUserId ? "model" : "user", m.Content))
                    .ToList();

                var builder = new StringBuilder();
                await foreach (var delta in aiService.StreamChatReplyAsync(history))
                {
                    builder.Append(delta);
                    await chatHubService.SendAiChunkAsync(roomId, delta);
                }

                replyContent = builder.Length > 0
                    ? builder.ToString()
                    : "🤖 I could not come up with a reply, please try again.";
            }

            var aiMessage = new Message
            {
                ChatRoomId = roomId,
                SenderId = aiUserId,
                Content = replyContent,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await messageRepository.AddAsync(aiMessage);
            await unitOfWork.CommitAsync();

            var createdMessage = await messageRepository.TableNoTracking
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == aiMessage.Id);

            var messageDto = mapper.Map<MessageDto>(createdMessage);
            await chatHubService.SendMessageToRoomAsync(roomId, messageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI reply failed for room {RoomId}", roomId);
            try
            {
                var messageRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<Message>>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                var fallbackMessage = new Message
                {
                    ChatRoomId = roomId,
                    SenderId = aiUserId,
                    Content = "😵 I ran into a problem while replying (the AI service may be busy or rate limited). Please try again in a bit.",
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                await messageRepository.AddAsync(fallbackMessage);
                await unitOfWork.CommitAsync();

                var createdFallback = await messageRepository.TableNoTracking
                    .Include(m => m.Sender)
                    .FirstOrDefaultAsync(m => m.Id == fallbackMessage.Id);

                await chatHubService.SendMessageToRoomAsync(roomId, mapper.Map<MessageDto>(createdFallback));
            }
            catch (Exception)
            {
            }
        }
        finally
        {
            try
            {
                await chatHubService.SendAiTypingAsync(roomId, false);
            }
            catch (Exception)
            {
            }
        }
    }
}
