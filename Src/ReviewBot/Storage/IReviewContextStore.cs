#region using

using System.Threading.Tasks;
using Review.Core.DataModel;

#endregion

namespace ReviewBot.Storage
{
    public interface IReviewContextStore
    {
        Task<ReviewContext> GetContext(string contextId);

        Task SaveContext(ReviewContext context);
    }
}