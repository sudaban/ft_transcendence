namespace Backend.Application.Abstractions;

public interface IAiChatResponder
{
    void QueueReply(int roomId, int aiUserId, int requestingUserId);
}
