#region using

using System;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Utility;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands.Review
{
    public class CurrentStatusCommand : ReviewCommand
    {
        public CurrentStatusCommand(IReviewContextStore contextStore)
            : base(contextStore)
        {
        }

        public override double GetMatchingScore(IActivity activity)
        {
            var messageActivity = activity.AsMessageActivity();
            if (messageActivity == null) return 0;

            if (!messageActivity.StartsWithRecipientMention()) return 0;

            var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
            return message.Equals("status", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
        }

        public override string PrintUsage(string myName)
        {
            return $"Print debt status of active reviewers: @{myName} status";
        }

        protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
        {
            return new CurrentStatusExecutable(this, turnContext, contextStore);
        }

        private class CurrentStatusExecutable : ReviewCommandExecutable
        {
            public CurrentStatusExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
                : base(command, turnContext, contextStore)
            {
            }

            protected override bool IsReadonly => true;

            protected override IActivity ExecuteReviewAction()
            {
                var activeReviewers = ReviewService.GetAllReviewers()
                                                   .Where(r => !r.IsSuspended)
                                                   .OrderByDescending(r => r.ReviewDebt)
                                                   .ThenBy(r => r.Name)
                                                   .ToList();

                if (activeReviewers.IsEmpty())
                {
                    return TurnContext.Activity.CreateReply("There are no active reviewers.");
                }

                var reply = TurnContext.Activity.CreateReply("Ordered by debt:").AppendNewline();
                foreach (var reviewer in activeReviewers)
                {
                    reply.AppendMention(new ChannelAccount(reviewer.Id, reviewer.Name))
                         .AppendText($" ({reviewer.Status}) Debt: {reviewer.ReviewDebt}")
                         .AppendNewline();
                }

                return reply;
            }
        }
    }
}