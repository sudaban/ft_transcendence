using Backend.Application.DTOs.Requests.ChatRooms;
using Backend.Application.DTOs.Responses.ChatRooms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(int roomId, SendMessageDto request);
    Task<IEnumerable<MessageDto>> GetRoomMessagesAsync(int roomId, int page = 1, int pageSize = 50);
    Task DeleteMessageAsync(int messageId);
    Task DeleteMessageForMeAsync(int messageId);
}
