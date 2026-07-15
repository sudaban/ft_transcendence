using Backend.Application.DTOs.Responses.ChatRooms;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface IChatHubService
{
    Task SendMessageToRoomAsync(int roomId, MessageDto message);

    Task SendAiTypingAsync(int roomId, bool isTyping);

    Task SendAiChunkAsync(int roomId, string delta);
}
