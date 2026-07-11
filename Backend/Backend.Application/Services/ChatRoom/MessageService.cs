using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.ChatRooms;
using Backend.Application.DTOs.Responses.ChatRooms;
using Backend.Application.Exceptions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Application.Services.ChatRoom;

public class MessageService : IMessageService
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<ChatRoomMember> _memberRepository;
    private readonly IChatHubService _chatHubService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public MessageService(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<ChatRoomMember> memberRepository,
        IChatHubService chatHubService,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _memberRepository = memberRepository;
        _chatHubService = chatHubService;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<MessageDto> SendMessageAsync(int roomId, SendMessageDto request)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        bool isMember = await _memberRepository.TableNoTracking
            .AnyAsync(m => m.ChatRoomId == roomId && m.UserId == currentUserId);

        if (!isMember)
            throw new UnAuthorizedAccessException("You are not a member of this chat room.");

        var message = new Message
        {
            ChatRoomId = roomId,
            SenderId = currentUserId,
            Content = request.Content,
            SentAt = System.DateTime.UtcNow,
            IsRead = false
        };

        await _messageRepository.AddAsync(message);
        
        // Odadaki tüm üyelerin IsHidden durumunu false yapalım (Sohbeti sildilerse bile yeni mesaj geldiği için geri gelsin)
        var members = await _memberRepository.Table
            .Where(m => m.ChatRoomId == roomId)
            .ToListAsync();
            
        foreach (var member in members)
        {
            member.IsHidden = false;
        }

        await _unitOfWork.CommitAsync();

        var createdMessage = await _messageRepository.TableNoTracking
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        var messageDto = _mapper.Map<MessageDto>(createdMessage);

        // SignalR ile odaya anlık mesaj iletimi
        await _chatHubService.SendMessageToRoomAsync(roomId, messageDto);

        return messageDto;
    }

    public async Task<IEnumerable<MessageDto>> GetRoomMessagesAsync(int roomId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var member = await _memberRepository.TableNoTracking
            .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == currentUserId);

        if (!isAdmin && member == null)
        {
            throw new UnAuthorizedAccessException("You cannot view messages of a room you are not a member of.");
        }

        var query = _messageRepository.TableNoTracking
            .Include(m => m.Sender)
            .Where(m => m.ChatRoomId == roomId)
            .Where(m => !m.DeletedByUsers.Any(dm => dm.UserId == currentUserId));

        if (member != null && member.ClearedAt.HasValue)
        {
            query = query.Where(m => m.SentAt > member.ClearedAt.Value);
        }

        var messages = await query.OrderBy(m => m.SentAt).ToListAsync();

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task DeleteMessageAsync(int messageId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new NotFoundException($"Message with ID {messageId} not found.");

        if (message.SenderId != currentUserId && !isAdmin)
        {
            throw new UnAuthorizedAccessException("You can only delete your own messages.");
        }

        await _messageRepository.DeleteAsync(message);
        await _unitOfWork.CommitAsync();
    }

    public async Task DeleteMessageForMeAsync(int messageId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var message = await _messageRepository.Table
            .Include(m => m.DeletedByUsers)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            throw new NotFoundException($"Message with ID {messageId} not found.");

        bool isAlreadyDeleted = message.DeletedByUsers.Any(dm => dm.UserId == currentUserId);
        if (!isAlreadyDeleted)
        {
            message.DeletedByUsers.Add(new DeletedMessage { UserId = currentUserId, MessageId = messageId });
            await _unitOfWork.CommitAsync();
        }
    }
}
