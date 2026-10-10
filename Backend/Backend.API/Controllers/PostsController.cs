using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromForm] CreatePostDto request)
    {
        var post = await _postService.CreatePostAsync(request);
        return Ok(post);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        await _postService.DeletePostAsync(id);
        return Ok(new { Message = "Post deleted successfully." });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetPostById(int id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        return Ok(post);
    }

    [HttpGet("feed")]
    [Authorize]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var feed = await _postService.GetFeedAsync(page, pageSize);
        return Ok(feed);
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserPosts(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var posts = await _postService.GetUserPostsAsync(userId, page, pageSize);
        return Ok(posts);
    }
}
