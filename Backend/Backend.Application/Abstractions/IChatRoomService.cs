using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Application.DTOs.Requests.ChatRooms;
using Backend.Application.DTOs.Responses.ChatRooms;

namespace Backend.Application.Abstractions;

public interface IChatRoomService
{
    Task<ChatRoomDto> GetChatRoomByIdAsync(int id);
    Task<IEnumerable<ChatRoomDto>> GetAllChatRoomsAsync();
    Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto request);
    Task DeleteChatRoomAsync(int id);
} 
