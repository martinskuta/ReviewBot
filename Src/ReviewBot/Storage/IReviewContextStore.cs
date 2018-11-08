#region using

using System.Threading.Tasks;
using Review.Core.DataModel;

#endregion

namespace ReviewBot.Storage
{
    public interface IReviewContextStore
    {
        Task<ReviewContext> GetContext(string contextid);

        Task SaveContext(ReviewContext context);
    }
}