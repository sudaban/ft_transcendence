using Backend.Application.Abstractions;
using Backend.Application.DTOs.Responses.ChatRooms;
using Backend.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Backend.API.Services;

public class ChatHubService : IChatHubService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatHubService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendMessageToRoomAsync(int roomId, MessageDto message)
    {
        await _hubContext.Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", message);
    }

    public async Task SendAiTypingAsync(int roomId, bool isTyping)
    {
        await _hubContext.Clients.Group($"Room_{roomId}").SendAsync("AiTyping", roomId, isTyping);
    }

    public async Task SendAiChunkAsync(int roomId, string delta)
    {
        await _hubContext.Clients.Group($"Room_{roomId}").SendAsync("AiMessageChunk", roomId, delta);
    }
}
