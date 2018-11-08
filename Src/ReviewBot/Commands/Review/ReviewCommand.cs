#region using

using Microsoft.Bot.Builder;
using ReviewBot.Storage;

#endregion

namespace ReviewBot.Commands.Review
{
    public abstract class ReviewCommand : Command
    {
        private readonly IReviewContextStore _contextStore;

        protected ReviewCommand(IReviewContextStore contextStore)
        {
            _contextStore = contextStore;
        }

        public override CommandExecutable CreateExecutable(ITurnContext turnContext)
        {
            return CreateReviewExecutable(turnContext, _contextStore);
        }

        protected abstract ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore);
    }
}