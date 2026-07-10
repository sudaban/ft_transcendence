using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface ILikeService
{
    Task LikePostAsync(int postId);
    Task UnlikePostAsync(int postId);
}
