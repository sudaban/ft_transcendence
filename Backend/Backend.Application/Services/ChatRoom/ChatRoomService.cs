using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Application.Abstractions;
using Backend.Application.DTOs.Requests.ChatRooms;
using Backend.Application.DTOs.Responses.ChatRooms;
using Backend.Application.Exceptions;
using Backend.Application.Extensions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Services.ChatRoom;

public class ChatRoomService : IChatRoomService
{
    private readonly IGenericRepository<Backend.Domain.Entities.ChatRoom> _chatRoomRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChatRoomService(IGenericRepository<Backend.Domain.Entities.ChatRoom> chatRoomRepository, IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _chatRoomRepository = chatRoomRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ChatRoomDto> GetChatRoomByIdAsync(int id)
    {
        var userId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        var isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var query = _chatRoomRepository.TableNoTracking.Where(cr => cr.Id == id);

        if (!isAdmin)
        {
            query = query.Where(cr => cr.Members.Any(m => m.UserId == userId));
        }

        var chatRoom = await query.FirstOrDefaultAsync();
        if (chatRoom == null)
            throw new NotFoundException($"Chat room with ID {id} not found or access denied.");

        return _mapper.Map<ChatRoomDto>(chatRoom);
    }

    public async Task<IEnumerable<ChatRoomDto>> GetAllChatRoomsAsync()
    {
        var userId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        var isAdmin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        var query = _chatRoomRepository.TableNoTracking;

        if (!isAdmin)
        {
            query = query.Where(cr => cr.Members.Any(m => m.UserId == userId));
        }

        var chatRooms = await query.ToListAsync();
        return _mapper.Map<IEnumerable<ChatRoomDto>>(chatRooms);
    }

    public async Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto request)
    {
        // Kullanıcı giriş yapmış mı kontrol ediliyor
        var userId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var chatRoom = new Backend.Domain.Entities.ChatRoom
        {
            IsGroup = request.IsGroup,
            CreatedAt = System.DateTime.UtcNow
        };

        await _chatRoomRepository.AddAsync(chatRoom);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<ChatRoomDto>(chatRoom);
    }

    public async Task DeleteChatRoomAsync(int id)
    {
        // Kullanıcı giriş yapmış mı kontrol ediliyor
        var userId = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var chatRoom = await _chatRoomRepository.GetByIdAsync(id);
        if (chatRoom == null)
            throw new NotFoundException($"ChatRoom with ID {id} not found.");

        await _chatRoomRepository.DeleteAsync(chatRoom);
        await _unitOfWork.CommitAsync();
    }
}
