using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Backend.API.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("posts/{postId}/comments")]
    public async Task<IActionResult> GetPostComments(int postId)
    {
        var comments = await _commentService.GetPostCommentsAsync(postId);
        return Ok(comments);
    }

    [HttpPost("posts/{postId}/comments")]
    public async Task<IActionResult> AddComment(int postId, [FromBody] CreateCommentDto request)
    {
        var comment = await _commentService.AddCommentAsync(postId, request);
        return CreatedAtAction(nameof(GetPostComments), new { postId = postId }, comment);
    }

    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        await _commentService.DeleteCommentAsync(commentId);
        return Ok(new { Message = "Comment deleted successfully" });
    }
}
