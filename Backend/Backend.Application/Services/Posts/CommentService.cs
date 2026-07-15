using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Posts;
using Backend.Application.DTOs.Responses.Posts;
using Backend.Application.Exceptions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Application.Services.Posts;

public class CommentService : ICommentService
{
    private readonly IGenericRepository<Comment> _commentRepository;
    private readonly IGenericRepository<Post> _postRepository;
    private readonly IGenericRepository<UserBlock> _userBlockRepository;
    private readonly IAiService _aiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public CommentService(
        IGenericRepository<Comment> commentRepository,
        IGenericRepository<Post> postRepository,
        IGenericRepository<UserBlock> userBlockRepository,
        IAiService aiService,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userBlockRepository = userBlockRepository;
        _aiService = aiService;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<CommentDto> AddCommentAsync(int postId, CreateCommentDto request)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var moderation = await _aiService.ModerateContentAsync(request.Content ?? string.Empty);
        if (!moderation.IsAllowed)
            throw new BadRequestException($"AI moderation blocked this comment: {moderation.Reason ?? "content violates platform rules"}");

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new NotFoundException($"Post with ID {postId} not found.");

        if (!isAdmin && post.UserId != currentUserId)
        {
            bool hasBlock = await _userBlockRepository.TableNoTracking
                .AnyAsync(ub => (ub.BlockerId == currentUserId && ub.BlockedId == post.UserId) ||
                                (ub.BlockerId == post.UserId && ub.BlockedId == currentUserId));
            
            if (hasBlock)
                throw new UnAuthorizedAccessException("You cannot comment on this post due to a block.");
        }

        var comment = new Comment
        {
            PostId = postId,
            UserId = currentUserId,
            Content = request.Content,
            CreatedAt = System.DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);
        await _unitOfWork.CommitAsync();

        var createdComment = await _commentRepository.TableNoTracking
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        return _mapper.Map<CommentDto>(createdComment);
    }

    public async Task DeleteCommentAsync(int commentId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var comment = await _commentRepository.Table
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException($"Comment with ID {commentId} not found.");

        // Kullanıcı kendi yorumunu, admin herhangi bir yorumu, post sahibi de kendi postuna atılan herhangi bir yorumu silebilir.
        if (comment.UserId != currentUserId && comment.Post.UserId != currentUserId && !isAdmin)
        {
            throw new UnAuthorizedAccessException("You are not authorized to delete this comment.");
        }

        await _commentRepository.DeleteAsync(comment);
        await _unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<CommentDto>> GetPostCommentsAsync(int postId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new NotFoundException($"Post with ID {postId} not found.");

        var commentsQuery = _commentRepository.TableNoTracking
            .Include(c => c.User)
            .Where(c => c.PostId == postId);

        if (!isAdmin)
        {
            var blockedIds = await _userBlockRepository.TableNoTracking
                .Where(ub => ub.BlockerId == currentUserId || ub.BlockedId == currentUserId)
                .Select(ub => ub.BlockerId == currentUserId ? ub.BlockedId : ub.BlockerId)
                .ToListAsync();

            if (blockedIds.Any())
            {
                commentsQuery = commentsQuery.Where(c => !blockedIds.Contains(c.UserId));
            }
        }

        var comments = await commentsQuery.OrderByDescending(c => c.CreatedAt).ToListAsync();

        return _mapper.Map<IEnumerable<CommentDto>>(comments);
    }
}
