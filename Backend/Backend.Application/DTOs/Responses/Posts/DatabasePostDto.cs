namespace Backend.Application.DTOs.Responses.Posts;

public record DatabasePostDto(
    string Id,
    string AuthorId,
    string CreatedAt,
    IReadOnlyCollection<string> Likes,
    IReadOnlyCollection<string> Comments,
    IReadOnlyCollection<string> Saves,
    int ViewsCount, 
    string Content,
    bool? IsVideo
);