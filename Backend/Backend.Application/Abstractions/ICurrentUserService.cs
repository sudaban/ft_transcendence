namespace Backend.Application.Abstractions;

public interface ICurrentUserService
{
    int UserId { get; }
    int? MaybeUserId { get; }
    bool IsAdmin { get; }
    void CheckIfAdmin();
}
