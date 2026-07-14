using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public record AiChatMessage(string Role, string Content);

public interface IAiService
{
    bool IsConfigured { get; }

    IAsyncEnumerable<string> StreamChatReplyAsync(IReadOnlyList<AiChatMessage> history, CancellationToken cancellationToken = default);
}
