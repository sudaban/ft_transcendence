using Backend.Application.DTOs.Requests.ChatRooms;
using Backend.Application.DTOs.Responses.ChatRooms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(int roomId, SendMessageDto request);
    Task<IEnumerable<MessageDto>> GetRoomMessagesAsync(int roomId);
    Task DeleteMessageAsync(int messageId);
    Task DeleteMessageForMeAsync(int messageId);
}
