#region using

using System;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Services;
using Review.Core.Utility;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

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

            var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
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

                var reply = TurnContext.Activity.CreateReply($"There are {allReviewers.Count} reviewers registered. Ordered debt:").AppendNewline();
                foreach (var reviewer in allReviewers.OrderByDescending(r => r.ReviewDebt))
                {
                    reply.AppendMention(new ChannelAccount(reviewer.Id, reviewer.Name))
                         .AppendText($" ({reviewer.Status}): ReviewCount: {reviewer.ReviewCount}, ReviewDebt: {reviewer.ReviewDebt}")
                         .AppendNewline();
                }

                return reply;
            }
        }
    }
}