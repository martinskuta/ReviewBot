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

        public override string PrintUsage(string myName)
        {
            return $"Suspend reviewer: '@{myName} suspend @reviewer1' or '@{myName}' suspend me' to suspend yourself";
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
                var reviewersToSuspend = messageActivity.GetUniqueMentionsExceptRecipient().Select(m => m.Mentioned).ToList();

                if (reviewersToSuspend.IsEmpty()) return SelfSuspend();

                return SuspendReviewer(reviewersToSuspend[0]);
            }

            private IActivity SelfSuspend()
            {
                try
                {
                    ReviewService.SuspendReviewer(TurnContext.Activity.From.Id);
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendMention(TurnContext.Activity.From).AppendText(", you are now suspended from reviews. Your review debt won't increase until resumed.");
                }
                catch (ReviewerNotRegisteredException)
                {
                    return CreateSorryYouAreNotRegisteredReply(TurnContext.Activity.From);
                }
                catch (ReviewerAlreadySuspendedException)
                {
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendMention(TurnContext.Activity.From).AppendText(", you are already suspended.");
                }
            }

            private IActivity SuspendReviewer(ChannelAccount reviewer)
            {
                try
                {
                    ReviewService.SuspendReviewer(reviewer.Id);
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendMention(reviewer).AppendText($" is now suspended from reviews. {reviewer.Name}'s review debt won't increase until resumed.");
                }
                catch (ReviewerNotRegisteredException)
                {
                    return CreateSorryReviewerNotRegisteredReply(TurnContext.Activity.From, reviewer);
                }
                catch (ReviewerAlreadySuspendedException)
                {
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendMention(reviewer).AppendText(" is already suspended.");
                }
            }
        }
    }
}