using Backend.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Backend.API.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class SavedPostsController : ControllerBase
{
    private readonly ISavedPostService _savedPostService;

    public SavedPostsController(ISavedPostService savedPostService)
    {
        _savedPostService = savedPostService;
    }

    [HttpGet("saved-posts")]
    public async Task<IActionResult> GetSavedPosts()
    {
        var posts = await _savedPostService.GetSavedPostsAsync();
        return Ok(posts);
    }

    [HttpPost("posts/{postId}/save")]
    public async Task<IActionResult> SavePost(int postId)
    {
        await _savedPostService.SavePostAsync(postId);
        return Ok(new { Message = "Post saved successfully" });
    }

    [HttpDelete("posts/{postId}/save")]
    public async Task<IActionResult> UnsavePost(int postId)
    {
        await _savedPostService.UnsavePostAsync(postId);
        return Ok(new { Message = "Post unsaved successfully" });
    }
}
