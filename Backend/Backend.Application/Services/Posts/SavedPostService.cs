using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Responses.Posts;
using Backend.Application.DTOs.Responses.Users;
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

    public async Task<IEnumerable<PostDto>> GetSavedPostsAsync(int page = 1, int pageSize = 20)
    {
        int currentUserId = _currentUserService.UserId;
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : (pageSize > 100 ? 100 : pageSize);

        var savedPosts = await _savedPostRepository.TableNoTracking
            .Where(sp => sp.UserId == currentUserId)
            .OrderByDescending(sp => sp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(sp => new
            {
                sp.Post.Id,
                sp.Post.User,
                sp.Post.ImageUrl,
                sp.Post.Content,
                sp.Post.ViewsCount,
                sp.Post.IsVideo,
                sp.Post.CreatedAt,
                LikesCount = sp.Post.Likes.Count(),
                CommentsCount = sp.Post.Comments.Count(),
                IsLiked = sp.Post.Likes.Any(l => l.UserId == currentUserId)
            })
            .ToListAsync();

        return savedPosts.Select(p => new PostDto
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
}
