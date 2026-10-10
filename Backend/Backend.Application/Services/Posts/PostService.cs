using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Posts;
using Backend.Application.DTOs.Responses.Posts;
using Backend.Application.DTOs.Responses.Users;
using Backend.Application.Exceptions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Application.Services.Posts;

public class PostService : IPostService
{
    private readonly IGenericRepository<Post> _postRepository;
    private readonly IGenericRepository<Follow> _followRepository;
    private readonly IGenericRepository<UserBlock> _userBlockRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAiService _aiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public PostService(
        IGenericRepository<Post> postRepository,
        IGenericRepository<Follow> followRepository,
        IGenericRepository<UserBlock> userBlockRepository,
        IFileUploadService fileUploadService,
        IAiService aiService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _fileUploadService = fileUploadService;
        _aiService = aiService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PostDto> CreatePostAsync(CreatePostDto request)
    {
        int currentUserId = _currentUserService.UserId;

        var moderation = await _aiService.ModerateContentAsync(request.Content ?? string.Empty);
        if (!moderation.IsAllowed)
            throw new BadRequestException($"AI moderation blocked this post: {moderation.Reason ?? "content violates platform rules"}");

        string? file_url = null;
        string? content_type = null;

        if (request.File != null)
        {
            using var stream = request.File.OpenReadStream();
            file_url = await _fileUploadService.UploadFileAsync(stream, request.File.FileName, request.File.ContentType);
            content_type = request.File.ContentType;
        }

        var post = BuildPostEntity(currentUserId, file_url, request.Content, content_type);

        await _postRepository.AddAsync(post);
        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<PostDto>(post);
        dto.IsLiked = false;
        return dto;
    }

    public async Task DeletePostAsync(int id)
    {
        int currentUserId = _currentUserService.UserId;
        bool isAdmin = _currentUserService.IsAdmin;

        var post = await _postRepository.GetByIdAsync(id);
        
        if (post == null)
            throw new NotFoundException($"Post with ID {id} not found.");

        if (post.UserId != currentUserId && !isAdmin)
            throw new UnAuthorizedAccessException("You can only delete your own posts.");

        await _postRepository.DeleteAsync(post);
        await _unitOfWork.CommitAsync();
    }

    public async Task<PostDto> GetPostByIdAsync(int id)
    {
        int currentUserId = _currentUserService.UserId;
        bool isAdmin = _currentUserService.IsAdmin;

        var postData = await _postRepository.TableNoTracking
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.UserId,
                p.User,
                p.ImageUrl,
                p.Content,
                p.ViewsCount,
                p.IsVideo,
                p.CreatedAt,
                LikesCount = p.Likes.Count(),
                CommentsCount = p.Comments.Count(),
                IsLiked = p.Likes.Any(l => l.UserId == currentUserId)
            })
            .FirstOrDefaultAsync();

        if (postData == null)
            throw new NotFoundException($"Post with ID {id} not found.");

        if (!isAdmin && postData.UserId != currentUserId)
        {
            bool hasBlock = await _userBlockRepository.TableNoTracking
                .AnyAsync(ub => (ub.BlockerId == currentUserId && ub.BlockedId == postData.UserId) ||
                                (ub.BlockerId == postData.UserId && ub.BlockedId == currentUserId));

            if (hasBlock)
                throw new UnAuthorizedAccessException("You cannot view this post.");
        }

        return new PostDto
        {
            Id = postData.Id,
            Author = _mapper.Map<UserDto>(postData.User),
            ImageUrl = postData.ImageUrl,
            Content = postData.Content,
            ViewsCount = postData.ViewsCount,
            IsVideo = postData.IsVideo,
            CreatedAt = postData.CreatedAt,
            LikesCount = postData.LikesCount,
            CommentsCount = postData.CommentsCount,
            IsLiked = postData.IsLiked
        };
    }

    public async Task<IEnumerable<PostDto>> GetFeedAsync(int page = 1, int pageSize = 20)
    {
        int currentUserId = _currentUserService.UserId;
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : (pageSize > 100 ? 100 : pageSize);

        var followingIds = await _followRepository.TableNoTracking
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        followingIds.Add(currentUserId);

        var blockedIds = await _userBlockRepository.TableNoTracking
            .Where(ub => ub.BlockerId == currentUserId || ub.BlockedId == currentUserId)
            .Select(ub => ub.BlockerId == currentUserId ? ub.BlockedId : ub.BlockerId)
            .ToListAsync();

        var feedIds = followingIds.Except(blockedIds).ToList();

        var posts = await _postRepository.TableNoTracking
            .Where(p => feedIds.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.User,
                p.ImageUrl,
                p.Content,
                p.ViewsCount,
                p.IsVideo,
                p.CreatedAt,
                LikesCount = p.Likes.Count(),
                CommentsCount = p.Comments.Count(),
                IsLiked = p.Likes.Any(l => l.UserId == currentUserId)
            })
            .ToListAsync();

        return posts.Select(p => new PostDto
        {
            Id = p.Id,
            Author = _mapper.Map<UserDto>(p.User),
            ImageUrl = p.ImageUrl,
            Content = p.Content,
            ViewsCount = p.ViewsCount,
            IsVideo = p.IsVideo,
            CreatedAt = p.CreatedAt,
            LikesCount = p.LikesCount,
            CommentsCount = p.CommentsCount,
            IsLiked = p.IsLiked
        });
    }

    public async Task<IEnumerable<PostDto>> GetUserPostsAsync(int userId, int page = 1, int pageSize = 20)
    {
        int currentUserId = _currentUserService.UserId;
        bool isAdmin = _currentUserService.IsAdmin;
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : (pageSize > 100 ? 100 : pageSize);

        if (!isAdmin && userId != currentUserId)
        {
            bool hasBlock = await _userBlockRepository.TableNoTracking
                .AnyAsync(ub => (ub.BlockerId == currentUserId && ub.BlockedId == userId) ||
                                (ub.BlockerId == userId && ub.BlockedId == currentUserId));

            if (hasBlock)
                return new List<PostDto>();
        }

        var posts = await _postRepository.TableNoTracking
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.User,
                p.ImageUrl,
                p.Content,
                p.ViewsCount,
                p.IsVideo,
                p.CreatedAt,
                LikesCount = p.Likes.Count(),
                CommentsCount = p.Comments.Count(),
                IsLiked = p.Likes.Any(l => l.UserId == currentUserId)
            })
            .ToListAsync();

        return posts.Select(p => new PostDto
        {
            Id = p.Id,
            Author = _mapper.Map<UserDto>(p.User),
            ImageUrl = p.ImageUrl,
            Content = p.Content,
            ViewsCount = p.ViewsCount,
            IsVideo = p.IsVideo,
            CreatedAt = p.CreatedAt,
            LikesCount = p.LikesCount,
            CommentsCount = p.CommentsCount,
            IsLiked = p.IsLiked
        });
    }

    
    private Post BuildPostEntity(int userId, string? fileUrl, string? content, string? contentType)
    {
        return new Post
        {
            UserId = userId,
            ImageUrl = fileUrl ?? string.Empty,
            Content = content,
            IsVideo = contentType != null && contentType.StartsWith("video/"),
            CreatedAt = DateTime.UtcNow
        };
    }
}
