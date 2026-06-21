namespace Backend.Application.DTOs.Requests.Comments
{
    public record CreateCommentRequestDto
    (
    int PostId,
    string Content
    );
}
