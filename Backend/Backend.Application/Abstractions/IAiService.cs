using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public record AiChatMessage(string Role, string Content);

public record AiModerationResult(bool IsAllowed, string? Reason);

public interface IAiService
{
    bool IsConfigured { get; }

    IAsyncEnumerable<string> StreamChatReplyAsync(IReadOnlyList<AiChatMessage> history, CancellationToken cancellationToken = default);

    Task<AiModerationResult> ModerateContentAsync(string content, CancellationToken cancellationToken = default);
}
