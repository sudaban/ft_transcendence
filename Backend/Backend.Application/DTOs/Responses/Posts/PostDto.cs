using Backend.Application.DTOs.Responses.Users;
using System;

namespace Backend.Application.DTOs.Responses.Posts;

public record PostDto(
    string Id,
    UserDto Author,     // Yukarıda oluşturduğumuz UserDto'yu kullanıyoruz
    string Content,
    DateTime CreatedAt,
    int LikesCount,     // Beğeni sayısını (Likes.Count) buraya çekeceğiz
    int RepostsCount,    // Kaydedilme sayısını (SavedByUsers.Count) buraya çekeceğiz
    int RepliesCount,
    int ViewsCount
);