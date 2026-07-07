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
    public async Task<IActionResult> GetAllChatRooms()
    {
        var chatRooms = await _chatRoomService.GetAllChatRoomsAsync();
        return Ok(chatRooms);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetChatRoomById(int id)
    {
        var chatRoom = await _chatRoomService.GetChatRoomByIdAsync(id);
        return Ok(chatRoom);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomDto request)
    {
        var chatRoom = await _chatRoomService.CreateChatRoomAsync(request);
        return Ok(chatRoom);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteChatRoom(int id)
    {
        await _chatRoomService.DeleteChatRoomAsync(id);
        return Ok(new { Message = "Chat room deleted successfully." });
    }
}
