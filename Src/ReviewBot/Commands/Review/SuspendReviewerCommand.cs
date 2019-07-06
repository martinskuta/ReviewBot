#region using

using System;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Services.Exceptions;
using Review.Core.Utility;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands.Review
{
    public class SuspendReviewerCommand : ReviewCommand
    {
        public SuspendReviewerCommand(IReviewContextStore contextStore)
            : base(contextStore)
        {
        }

        public override double GetMatchingScore(IActivity activity)
        {
            var messageActivity = activity.AsMessageActivity();
            if (messageActivity == null) return 0;

            if (!messageActivity.StartsWithRecipientMention()) return 0;

            var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
            var mentions = messageActivity.GetUniqueMentionsExceptRecipient();

            if (message.StartsWith("suspend me", StringComparison.InvariantCultureIgnoreCase) && mentions.IsEmpty()) return 1;

            return message.StartsWith("suspend", StringComparison.InvariantCultureIgnoreCase) && mentions.Count == 1 ? 1 : 0;
        }

        public override string[] PrintUsages(string myName)
        {
            return new[]
            {
                $"@{myName} suspend @reviewer",
                $"@{myName} suspend me"
            };
        }

        public override string Name()
        {
            return "Suspend reviewer";
        }

        public override string Description()
        {
            return "Way to change status of a reviewer to inactive. Inactive reviewers are NOT considered when looking for a reviewer and their debt does NOT increase. Use it when you are on vacations or when somebody leaves the team for example.";
        }

        protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
        {
            return new SuspendReviewerExecutable(this, turnContext, contextStore);
        }

        private class SuspendReviewerExecutable : ReviewCommandExecutable
        {
            public SuspendReviewerExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
                : base(command, turnContext, contextStore)
            {
            }

            protected override bool IsReadonly => false;

            protected override IActivity ExecuteReviewAction()
            {
                var messageActivity = TurnContext.Activity.AsMessageActivity();
                var reviewerToSuspend = messageActivity.GetUniqueMentionsExceptRecipient().FirstOrDefault()?.Mentioned;

                return reviewerToSuspend == null ? SelfSuspend() : SuspendReviewer(reviewerToSuspend);
            }

            private IActivity SelfSuspend()
            {
                try
                {
                    ReviewService.SuspendReviewer(TurnContext.Activity.From.Id);
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendMention(TurnContext.Activity.From).AppendText(" enjoy your time off! Your review debt won't increase until you are back.");
                }
                catch (ReviewerNotRegisteredException)
                {
                    return CreateSorryYouAreNotRegisteredReply();
                }
                catch (ReviewerAlreadySuspendedException)
                {
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Yeah yeah, I know that already.");
                }
            }

            private IActivity SuspendReviewer(ChannelAccount reviewer)
            {
                try
                {
                    ReviewService.SuspendReviewer(reviewer.Id);
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendMention(reviewer).AppendText(" enjoy your time off! Your review debt won't increase until you are back.");
                }
                catch (ReviewerNotRegisteredException)
                {
                    return CreateSorryReviewerNotRegisteredReply(reviewer);
                }
                catch (ReviewerAlreadySuspendedException)
                {
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Yeah yeah, I know that already.");
                }
            }
        }
    }
}