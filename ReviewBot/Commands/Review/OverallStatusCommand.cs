using System;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Services;
using Review.Core.Utility;
using ReviewBot.Storage;
using ReviewBot.Utility;

namespace ReviewBot.Commands.Review
{
    public class OverallStatusCommand : ReviewCommand
    {
        public OverallStatusCommand(IReviewContextStore contextStore)
            : base(contextStore)
        {
        }

        public override double GetMatchingScore(IActivity activity)
        {
            var messageActivity = activity.AsMessageActivity();
            if (messageActivity == null)
            {
                return 0;
            }

            if (!messageActivity.StartsWithRecipientMention())
            {
                return 0;
            }

            var message = messageActivity.RemoveRecipientMention().Trim();
            return message.Equals("status", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
        }

        public override string PrintUsage(string myName)
        {
            return $"Print overall status: @{myName} status";
        }

        protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
        {
            return new OverallStatusExecutable(this, turnContext, contextStore);
        }

        private class OverallStatusExecutable : ReviewCommandExecutable
        {
            public OverallStatusExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
                : base(command, turnContext, contextStore)
            {
            }

            protected override bool IsReadonly => true;

            protected override IActivity Execute(IReviewService reviewService)
            {
                var allReviewers = reviewService.GetAllReviewers();

                if (allReviewers.IsEmpty())
                {
                    return TurnContext.Activity.CreateReply("There are no reviewers registered yet.");
                }

                var sb = new StringBuilder($"There are {allReviewers.Count} reviewers registered. Ordered debt:");
                sb.AppendLine();
                sb.AppendLine(new string('-', sb.Length));
                foreach (var reviewer in allReviewers.OrderByDescending(r => r.ReviewDebt))
                {
                    //TODO Use user's name, by my mapping the reviewer id to channelAccount and using it in mention?
                    sb.AppendLine(reviewer.ToString());
                }

                return TurnContext.Activity.CreateReply(sb.ToString());
            }
        }
    }
}