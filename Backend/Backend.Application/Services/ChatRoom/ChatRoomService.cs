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
        var user_id = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        var is_admin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        IQueryable<Backend.Domain.Entities.ChatRoom> query = _chatRoomRepository.TableNoTracking
            .Include(cr => cr.Members)
                .ThenInclude(crm => crm.User)
            .Where(cr => cr.Id == id);

        if (!is_admin)
        {
            query = query.Where(cr => cr.Members.Any(m => m.UserId == user_id));
        }

        var chat_room = await query.FirstOrDefaultAsync();
        if (chat_room == null)
            throw new NotFoundException($"Chat room with ID {id} not found or access denied.");

        return _mapper.Map<ChatRoomDto>(chat_room);
    }

    public async Task<IEnumerable<ChatRoomDto>> GetAllChatRoomsAsync()
    {
        var user_id = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();
        var is_admin = _httpContextAccessor.HttpContext!.User.IsAdmin();

        IQueryable<Backend.Domain.Entities.ChatRoom> query = _chatRoomRepository.TableNoTracking
            .Include(cr => cr.Members)
                .ThenInclude(crm => crm.User);

        if (!is_admin)
        {
            query = query.Where(cr => cr.Members.Any(m => m.UserId == user_id && !m.IsHidden));
        }

        var chat_rooms = await query.ToListAsync();
        return _mapper.Map<IEnumerable<ChatRoomDto>>(chat_rooms);
    }

    public async Task<IEnumerable<ChatRoomDto>> GetMyChatRoomsAsync()
    {
        var user_id = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        IQueryable<Backend.Domain.Entities.ChatRoom> query = _chatRoomRepository.TableNoTracking
            .Include(cr => cr.Members)
                .ThenInclude(crm => crm.User)
            .Where(cr => cr.Members.Any(m => m.UserId == user_id && !m.IsHidden));

        var chat_rooms = await query.ToListAsync();
        return _mapper.Map<IEnumerable<ChatRoomDto>>(chat_rooms);
    }

    public async Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto request)
    {
        var user_id = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var chat_room = new Backend.Domain.Entities.ChatRoom
        {
            IsGroup = request.IsGroup,
            CreatedAt = System.DateTime.UtcNow
        };

        chat_room.Members.Add(new ChatRoomMember
        {
            UserId = user_id,
            JoinedAt = System.DateTime.UtcNow
        });

        if (request.TargetUserId.HasValue)
        {
            chat_room.Members.Add(new ChatRoomMember
            {
                UserId = request.TargetUserId.Value,
                JoinedAt = System.DateTime.UtcNow
            });
        }

        await _chatRoomRepository.AddAsync(chat_room);
        await _unitOfWork.CommitAsync();

        var created_chat_room = await _chatRoomRepository.TableNoTracking
            .Include(cr => cr.Members)
                .ThenInclude(crm => crm.User)
            .FirstOrDefaultAsync(cr => cr.Id == chat_room.Id);

        return _mapper.Map<ChatRoomDto>(created_chat_room);
    }

    public async Task DeleteChatRoomAsync(int id)
    {
        var user_id = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var chat_room = await _chatRoomRepository.GetByIdAsync(id);
        if (chat_room == null)
            throw new NotFoundException($"ChatRoom with ID {id} not found.");

        await _chatRoomRepository.DeleteAsync(chat_room);
        await _unitOfWork.CommitAsync();
    }

    public async Task HideChatRoomAsync(int id)
    {
        var user_id = _httpContextAccessor.HttpContext!.User.GetCurrentUserId();

        var chat_room = await _chatRoomRepository.Table
            .Include(cr => cr.Members)
            .FirstOrDefaultAsync(cr => cr.Id == id);

        if (chat_room == null)
            throw new NotFoundException($"ChatRoom with ID {id} not found.");

        var member = chat_room.Members.FirstOrDefault(m => m.UserId == user_id);
        if (member == null)
            throw new UnAuthorizedAccessException("You are not a member of this chat room.");

        member.IsHidden = true;
        member.ClearedAt = System.DateTime.UtcNow;
        await _unitOfWork.CommitAsync();
    }
}
