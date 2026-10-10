using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.ChatRooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatRoomsController : ControllerBase
{
    private readonly IChatRoomService _chatRoomService;

    public ChatRoomsController(IChatRoomService chatRoomService)
    {
        _chatRoomService = chatRoomService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllChatRooms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var chat_rooms = await _chatRoomService.GetAllChatRoomsAsync(page, pageSize);
        return Ok(chat_rooms);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyChatRooms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var chat_rooms = await _chatRoomService.GetMyChatRoomsAsync(page, pageSize);
        return Ok(chat_rooms);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetChatRoomById(int id)
    {
        var chat_room = await _chatRoomService.GetChatRoomByIdAsync(id);
        return Ok(chat_room);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomDto request)
    {
        var chat_room = await _chatRoomService.CreateChatRoomAsync(request);
        return Ok(chat_room);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteChatRoom(int id)
    {
        await _chatRoomService.DeleteChatRoomAsync(id);
        return Ok(new { Message = "Chat room deleted successfully." });
    }

    [HttpDelete("{id}/hide")]
    [Authorize]
    public async Task<IActionResult> HideChatRoom(int id)
    {
        await _chatRoomService.HideChatRoomAsync(id);
        return Ok(new { Message = "Chat room hidden for user." });
    }
}
