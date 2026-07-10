using Backend.Application.DTOs.Responses.ChatRooms;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface IChatHubService
{
    Task SendMessageToRoomAsync(int roomId, MessageDto message);
}
