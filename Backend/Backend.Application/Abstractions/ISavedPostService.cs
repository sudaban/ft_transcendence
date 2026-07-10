using Backend.Application.DTOs.Responses.Posts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface ISavedPostService
{
    Task SavePostAsync(int postId);
    Task UnsavePostAsync(int postId);
    Task<IEnumerable<PostDto>> GetSavedPostsAsync();
}
