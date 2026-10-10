using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Responses.Posts;
using Backend.Application.Exceptions;
using Backend.Domain.Entities;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public SavedPostService(
        IGenericRepository<SavedPost> savedPostRepository,
        IGenericRepository<Post> postRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _savedPostRepository = savedPostRepository;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task SavePostAsync(int postId)
    {
        int currentUserId = _currentUserService.UserId;

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new NotFoundException($"Post with ID {postId} not found.");

        var existingSavedPost = await _savedPostRepository.Table
            .FirstOrDefaultAsync(sp => sp.PostId == postId && sp.UserId == currentUserId);

        if (existingSavedPost != null)
        {
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
        int currentUserId = _currentUserService.UserId;

        var savedPost = await _savedPostRepository.Table
            .FirstOrDefaultAsync(sp => sp.PostId == postId && sp.UserId == currentUserId);

        if (savedPost == null)
            throw new NotFoundException("Saved post record not found.");

        await _savedPostRepository.DeleteAsync(savedPost);
        await _unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<PostDto>> GetSavedPostsAsync()
    {
        int currentUserId = _currentUserService.UserId;

        var savedPosts = await _savedPostRepository.TableNoTracking
            .Include(sp => sp.Post)
                .ThenInclude(p => p.User)
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
