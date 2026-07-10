using Backend.Application.DTOs.Requests.Posts;
using Backend.Application.DTOs.Responses.Posts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface ICommentService
{
    Task<CommentDto> AddCommentAsync(int postId, CreateCommentDto request);
    Task DeleteCommentAsync(int commentId);
    Task<IEnumerable<CommentDto>> GetPostCommentsAsync(int postId);
}
