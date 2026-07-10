using Backend.Application.Abstractions;
using Backend.Application.Exceptions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Backend.Application.Services.Posts;

public class PostLikeService : ILikeService
{
    private readonly IGenericRepository<PostLike> _postLikeRepository;
    private readonly IGenericRepository<Post> _postRepository;
    private readonly IGenericRepository<UserBlock> _userBlockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PostLikeService(
        IGenericRepository<PostLike> postLikeRepository,
        IGenericRepository<Post> postRepository,
        IGenericRepository<UserBlock> userBlockRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _postLikeRepository = postLikeRepository;
        _postRepository = postRepository;
        _userBlockRepository = userBlockRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LikePostAsync(int postId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new NotFoundException($"Post with ID {postId} not found.");

        if (!isAdmin && post.UserId != currentUserId)
        {
            bool hasBlock = await _userBlockRepository.TableNoTracking
                .AnyAsync(ub => (ub.BlockerId == currentUserId && ub.BlockedId == post.UserId) ||
                                (ub.BlockerId == post.UserId && ub.BlockedId == currentUserId));
            
            if (hasBlock)
                throw new UnAuthorizedAccessException("You cannot like this post due to a block.");
        }

        bool alreadyLiked = await _postLikeRepository.TableNoTracking
            .AnyAsync(pl => pl.PostId == postId && pl.UserId == currentUserId);

        if (alreadyLiked)
            return ;

        var postLike = new PostLike
        {
            PostId = postId,
            UserId = currentUserId,
            CreatedAt = System.DateTime.UtcNow
        };

        await _postLikeRepository.AddAsync(postLike);
        await _unitOfWork.CommitAsync();
    }

    public async Task UnlikePostAsync(int postId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var postLike = await _postLikeRepository.Table
            .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == currentUserId);

        if (postLike == null)
            throw new NotFoundException("Like record not found.");

        await _postLikeRepository.DeleteAsync(postLike);
        await _unitOfWork.CommitAsync();
    }
}
