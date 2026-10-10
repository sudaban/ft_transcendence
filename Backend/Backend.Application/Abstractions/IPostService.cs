using Backend.Application.DTOs.Requests.Posts;
using Backend.Application.DTOs.Responses.Posts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface IPostService
{
    Task<PostDto> CreatePostAsync(CreatePostDto request);
    Task DeletePostAsync(int id);
    Task<PostDto> GetPostByIdAsync(int id);
    Task<IEnumerable<PostDto>> GetFeedAsync(int page = 1, int pageSize = 20);
    Task<IEnumerable<PostDto>> GetUserPostsAsync(int userId, int page = 1, int pageSize = 20);
}
