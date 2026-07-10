using Backend.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Backend.API.Controllers;

[Authorize]
[ApiController]
[Route("api/posts/{postId}/likes")]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpPost]
    public async Task<IActionResult> LikePost(int postId)
    {
        await _likeService.LikePostAsync(postId);
        return Ok(new { Message = "Post liked successfully" });
    }

    [HttpDelete]
    public async Task<IActionResult> UnlikePost(int postId)
    {
        await _likeService.UnlikePostAsync(postId);
        return Ok(new { Message = "Post unliked successfully" });
    }
}
