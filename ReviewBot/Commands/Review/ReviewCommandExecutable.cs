#region using

using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Services;
using ReviewBot.Storage;

#endregion

namespace ReviewBot.Commands.Review
{
    public abstract class ReviewCommandExecutable : CommandExecutable
    {
        private readonly IReviewContextStore _contextStore;
        private IActivity _reply;

        protected ReviewCommandExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
            : base(command, turnContext)
        {
            _contextStore = contextStore;
        }

        public override async Task Execute()
        {
            var reviewContext = await _contextStore.GetContext("contextId");

            _reply = Execute(new ReviewService(reviewContext));

            await _contextStore.SaveContext(reviewContext);
        }

        public override IActivity GetReply() => _reply;

        protected abstract IActivity Execute(IReviewService reviewService);
    }
}