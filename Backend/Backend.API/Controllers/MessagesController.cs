using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.ChatRooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Backend.API.Controllers;

[Authorize]
[ApiController]
[Route("api/chat-rooms/{roomId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoomMessages(int roomId)
    {
        var messages = await _messageService.GetRoomMessagesAsync(roomId);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(int roomId, [FromBody] SendMessageDto request)
    {
        var message = await _messageService.SendMessageAsync(roomId, request);
        return Ok(message);
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(int roomId, int messageId)
    {
        // roomId is just for routing aesthetics here, but we pass messageId to service
        await _messageService.DeleteMessageAsync(messageId);
        return Ok(new { Message = "Message deleted successfully" });
    }

    [HttpDelete("{messageId}/for-me")]
    public async Task<IActionResult> DeleteMessageForMe(int roomId, int messageId)
    {
        await _messageService.DeleteMessageForMeAsync(messageId);
        return Ok(new { Message = "Message deleted for user successfully" });
    }
}
