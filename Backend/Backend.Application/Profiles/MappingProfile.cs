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
            //Requests
            CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());

            CreateMap<CreatePostRequestDto, Post>();
            CreateMap<UpdateProfileRequestDto, User>();
            CreateMap<CreateChatRoomDto, ChatRoom>();

            // Dışarıya Gönderilecek Temel Veriler
            CreateMap<User, UserDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id.ToString()))
                .ForCtorParam("Handle", opt => opt.MapFrom(src => $"@{src.Username}"))
                .ForCtorParam("Avatar", opt => opt.MapFrom(src => src.ProfilePictureUrl ?? ""))
                .ForCtorParam("FullName", opt => opt.MapFrom(src => src.FullName))
                .ForCtorParam("Bio", opt => opt.MapFrom(src => src.Bio))
                .ForCtorParam("FollowersCount", opt => opt.MapFrom(src => src.FollowedBy.Count))
                .ForCtorParam("FollowingCount", opt => opt.MapFrom(src => src.Following.Count))
                .ForCtorParam("PostsCount", opt => opt.MapFrom(src => src.Posts.Count));
            
            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments.Count));

            CreateMap<ChatRoom, ChatRoomDto>();

            // ==========================================
            //        Detaylı Veritabanı Verileri
            // ==========================================

            //Post -> DatabasePostDto Dönüşümü
            CreateMap<Post, DatabasePostDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id.ToString()))
                .ForCtorParam("AuthorId", opt => opt.MapFrom(src => src.UserId.ToString()))
                .ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt.ToString("0"))) // Tarihi frontend'in beklediği ISO standart metnine çeviriyoruz
                .ForCtorParam("Likes", opt => opt.MapFrom(src => src.Likes.Select(l => l.UserId.ToString()).ToList())) // Sadece ID'leri çektik
                .ForCtorParam("Comments", opt => opt.MapFrom(src => src.Comments.Select(c => c.Id.ToString()).ToList()))
                .ForCtorParam("Saves", opt => opt.MapFrom(src => src.SavedByUsers.Select(s => s.UserId.ToString()).ToList()));

            //User -> DatabaseUserDto Dönüşümü
            CreateMap<User, DatabaseUserDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id.ToString()))
                .ForCtorParam("FollowersCount", opt => opt.MapFrom(src => src.FollowedBy.Count))
                .ForCtorParam("Following", opt => opt.MapFrom(src => src.Following.Select(f => f.FollowingId.ToString()).ToList()))
                .ForCtorParam("BlockedUsers", opt => opt.MapFrom(src => src.BlockedUsers.Select(b => b.BlockedId.ToString()).ToList()))
                .ForCtorParam("LikedPosts", opt => opt.MapFrom(src => src.LikedPosts.Select(l => l.PostId.ToString()).ToList()))
                .ForCtorParam("SavedPosts", opt => opt.MapFrom(src => src.SavedPosts.Select(s => s.PostId.ToString()).ToList()))
                .ForCtorParam("CommentedPosts", opt => opt.MapFrom(src => src.Comments.Select(c => c.PostId.ToString()).Distinct().ToList()))
                .ForCtorParam("ActiveDMs", opt => opt.MapFrom(src => src.ChatRoomMemberships.Select(c => c.ChatRoomId.ToString()).ToList()))
                .ForCtorParam("Posts", opt => opt.MapFrom(src => src.Posts.Select(p => p.Id.ToString()).ToList()))
                .ForCtorParam("TermsAccepted", opt => opt.MapFrom(src => src.IsTosAccepted))
                .ForCtorParam("RegistrationDate", opt => opt.MapFrom(src => src.CreatedAt.ToString("O")))
                .ForCtorParam("FollowRequestTime", opt => opt.MapFrom(src => (string?)null)); // Veritabanımızda şu an takip isteği süresi yok, boş bırakıyoruz

        }
    }
}
