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
    public class MakeAvailableCommand : ReviewCommand
    {
        public MakeAvailableCommand(IReviewContextStore contextStore)
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

            if (mentions.IsEmpty() && message.Equals("I am back", StringComparison.InvariantCultureIgnoreCase)) return 1;

            return mentions.Count == 1 && message.EndsWith("is back", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
        }

        public override string PrintUsage(string myName)
        {
            return $"Make available: '@{myName} @reviewer1 is back' or '@{myName} I am back' to make yourself available";
        }

        protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
        {
            return new ResumeReviewerExecutable(this, turnContext, contextStore);
        }

        private class ResumeReviewerExecutable : ReviewCommandExecutable
        {
            public ResumeReviewerExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
                : base(command, turnContext, contextStore)
            {
            }

            protected override bool IsReadonly => false;

            protected override IActivity ExecuteReviewAction()
            {
                var messageActivity = TurnContext.Activity.AsMessageActivity();
                var reviewerToResume = messageActivity.GetUniqueMentionsExceptRecipient().FirstOrDefault()?.Mentioned;

                return reviewerToResume == null ? SelfResume() : ResumeReviewer(reviewerToResume);
            }

            private IActivity ResumeReviewer(ChannelAccount reviewer)
            {
                try
                {
                    ReviewService.MakeReviewerAvailable(reviewer.Id);
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Welcome back ").AppendMention(reviewer).AppendText("! Great to see you doing reviews again.");
                }
                catch (ReviewerNotRegisteredException)
                {
                    return CreateSorryReviewerNotRegisteredReply(reviewer);
                }
                catch (ReviewerAlreadyAvailableException)
                {
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Yeah yeah, I know that already.");
                }
            }

            private IActivity SelfResume()
            {
                try
                {
                    ReviewService.MakeReviewerAvailable(TurnContext.Activity.From.Id);
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Welcome back ").AppendMention(TurnContext.Activity.From).AppendText("! Great to see you doing reviews again.");
                }
                catch (ReviewerNotRegisteredException)
                {
                    return CreateSorryYouAreNotRegisteredReply();
                }
                catch (ReviewerAlreadyAvailableException)
                {
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Yeah yeah, I know that already.");
                }
            }
        }
    }
}