using Backend.Domain.Entities;
using AutoMapper;
using Backend.Application.DTOs.Responses.Users;
using Backend.Application.DTOs.Requests.Auth;
using Backend.Application.DTOs.Requests.Posts;
using Backend.Application.DTOs.Requests.Users;
using Backend.Application.DTOs.Responses.Posts;
using Backend.Application.DTOs.Responses.ChatRooms;
using Backend.Application.DTOs.Requests.ChatRooms;

namespace Backend.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());

            CreateMap<UpdateProfileRequestDto, User>();
            CreateMap<CreateChatRoomDto, ChatRoom>();
        
            CreateMap<User, UserDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id.ToString()))
                .ForCtorParam("Handle", opt => opt.MapFrom(src => $"@{src.Username}"))
                .ForCtorParam("Avatar", opt => opt.MapFrom(src => src.ProfilePictureUrl ?? ""))
                .ForCtorParam("FullName", opt => opt.MapFrom(src => src.FullName))
                .ForCtorParam("Bio", opt => opt.MapFrom(src => src.Bio))
                .ForCtorParam("FollowersCount", opt => opt.MapFrom(src => src.FollowedBy.Count))
                .ForCtorParam("FollowingCount", opt => opt.MapFrom(src => src.Following.Count))
                .ForCtorParam("PostsCount", opt => opt.MapFrom(src => src.Posts.Count))
                .ForCtorParam("IsTwoFactorEnabled", opt => opt.MapFrom(src => src.IsTwoFactorEnabled))
                .ForCtorParam("IsOnline", opt => opt.MapFrom(src => src.IsOnline))
                .ForCtorParam("LastSeenAt", opt => opt.MapFrom(src => src.LastSeenAt))
                .ForCtorParam("IsAiAssistant", opt => opt.MapFrom(src => src.IsAiAssistant))
                .ForCtorParam("IsBanned", opt => opt.MapFrom(src => src.IsBanned))
                .ForCtorParam("IsDeleted", opt => opt.MapFrom(src => src.IsDeleted));
            
            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments.Count))
                .ForMember(dest => dest.IsLiked, opt => opt.Ignore());

            CreateMap<ChatRoom, ChatRoomDto>()
                .ForCtorParam("Members", opt => opt.MapFrom(src => src.Members.Select(m => m.User).ToList()));
            CreateMap<Comment, CommentDto>();
            CreateMap<Message, MessageDto>();
 

        }
    }
}
