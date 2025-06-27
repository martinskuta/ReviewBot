#region using

using System.Collections.Generic;
using System.Threading.Tasks;
using Review.Core.DataModel;
using ReviewBot.Storage;

#endregion

namespace ReviewBot.Tests;

/// <summary>
///     Used for test purposes only.
/// </summary>
public class MemoryReviewContextStore : IReviewContextStore
{
    private readonly IDictionary<string, ReviewContext> _contexts = new Dictionary<string, ReviewContext>();

    public Task<ReviewContext> GetContext(string contextId)
    {
        if (!_contexts.TryGetValue(contextId, out var context))
        {
            context = new ReviewContext { Id = contextId, ETag = contextId };
            _contexts.Add(contextId, context);
        }

        return Task.FromResult(context);
    }

    public Task SaveContext(ReviewContext context)
    {
        _contexts[context.Id] = context;
        return Task.CompletedTask;
    }
}