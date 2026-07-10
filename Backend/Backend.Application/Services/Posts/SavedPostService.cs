using AutoMapper;
using Backend.Application.Abstractions;
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

public class SavedPostService : ISavedPostService
{
    private readonly IGenericRepository<SavedPost> _savedPostRepository;
    private readonly IGenericRepository<Post> _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public SavedPostService(
        IGenericRepository<SavedPost> savedPostRepository,
        IGenericRepository<Post> postRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _savedPostRepository = savedPostRepository;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task SavePostAsync(int postId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var postExists = await _postRepository.TableNoTracking.AnyAsync(p => p.Id == postId);
        if (!postExists)
            throw new NotFoundException($"Post with ID {postId} not found.");

        var existingSavedPost = await _savedPostRepository.Table
            .FirstOrDefaultAsync(sp => sp.PostId == postId && sp.UserId == currentUserId);

        if (existingSavedPost != null)
        {
            await _savedPostRepository.DeleteAsync(existingSavedPost);
            await _unitOfWork.CommitAsync();
            return;
        }

        var savedPost = new SavedPost
        {
            PostId = postId,
            UserId = currentUserId,
            CreatedAt = System.DateTime.UtcNow
        };

        await _savedPostRepository.AddAsync(savedPost);
        await _unitOfWork.CommitAsync();
    }

    public async Task UnsavePostAsync(int postId)
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var savedPost = await _savedPostRepository.Table
            .FirstOrDefaultAsync(sp => sp.PostId == postId && sp.UserId == currentUserId);

        if (savedPost == null)
            throw new NotFoundException("Saved post record not found.");

        await _savedPostRepository.DeleteAsync(savedPost);
        await _unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<PostDto>> GetSavedPostsAsync()
    {
        int currentUserId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var savedPosts = await _savedPostRepository.TableNoTracking
            .Include(sp => sp.Post)
                .ThenInclude(p => p.User)
                    .ThenInclude(u => u.FollowedBy)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.User)
                    .ThenInclude(u => u.Following)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.User)
                    .ThenInclude(u => u.Posts)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.Likes)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.Comments)
            .Where(sp => sp.UserId == currentUserId)
            .OrderByDescending(sp => sp.CreatedAt)
            .Select(sp => sp.Post)
            .ToListAsync();

        return savedPosts.Select(p =>
        {
            var dto = _mapper.Map<PostDto>(p);
            dto.IsLiked = p.Likes.Any(l => l.UserId == currentUserId);
            return dto;
        });
    }
}
