using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.Posts;
using Backend.Application.DTOs.Responses.Posts;
using Backend.Application.Exceptions;
using Backend.Application.Extensions;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public PostService(
        IGenericRepository<Post> postRepository,
        IGenericRepository<Follow> followRepository,
        IGenericRepository<UserBlock> userBlockRepository,
        IFileUploadService fileUploadService,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _fileUploadService = fileUploadService;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<PostDto> CreatePostAsync(CreatePostDto request)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        // using var stream = request.File.OpenReadStream();
        // var fileUrl = await _fileUploadService.UploadFileAsync(stream, request.File.FileName, request.File.ContentType);
        // var post = BuildPostEntity(currentUserId, fileUrl, request.Content, request.File.ContentType);

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
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

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
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var post = await _postRepository.TableNoTracking
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            throw new NotFoundException($"Post with ID {id} not found.");

        if (!isAdmin && post.UserId != currentUserId)
        {
            bool hasBlock = await _userBlockRepository.TableNoTracking
                .AnyAsync(ub => (ub.BlockerId == currentUserId && ub.BlockedId == post.UserId) ||
                                (ub.BlockerId == post.UserId && ub.BlockedId == currentUserId));

            if (hasBlock)
                throw new UnAuthorizedAccessException("You cannot view this post.");
        }

        var dto = _mapper.Map<PostDto>(post);
        dto.IsLiked = post.Likes.Any(l => l.UserId == currentUserId);
        return dto;
    }

    public async Task<IEnumerable<PostDto>> GetFeedAsync()
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

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
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .Where(p => feedIds.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(p =>
        {
            var dto = _mapper.Map<PostDto>(p);
            dto.IsLiked = p.Likes.Any(l => l.UserId == currentUserId);
            return dto;
        });
    }

    public async Task<IEnumerable<PostDto>> GetUserPostsAsync(int userId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        bool isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        if (!isAdmin && userId != currentUserId)
        {
            bool hasBlock = await _userBlockRepository.TableNoTracking
                .AnyAsync(ub => (ub.BlockerId == currentUserId && ub.BlockedId == userId) ||
                                (ub.BlockerId == userId && ub.BlockedId == currentUserId));

            if (hasBlock)
                return new List<PostDto>();
        }

        var posts = await _postRepository.TableNoTracking
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(p =>
        {
            var dto = _mapper.Map<PostDto>(p);
            dto.IsLiked = p.Likes.Any(l => l.UserId == currentUserId);
            return dto;
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
