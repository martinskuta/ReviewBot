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
    public class ResumeReviewerCommand : ReviewCommand
    {
        public ResumeReviewerCommand(IReviewContextStore contextStore)
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

            if (message.StartsWith("resume me", StringComparison.InvariantCultureIgnoreCase) && mentions.IsEmpty()) return 1;

            return message.StartsWith("resume", StringComparison.InvariantCultureIgnoreCase) && mentions.Count == 1 ? 1 : 0;
        }

        public override string PrintUsage(string myName)
        {
            return $"Resume reviewer: '@{myName} resume @reviewer1' or '@{myName}' resume me' to resume yourself";
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
                var reviewersToResume = messageActivity.GetUniqueMentionsExceptRecipient().Select(m => m.Mentioned).ToList();

                if (reviewersToResume.IsEmpty()) return SelfResume();

                return ResumeReviewer(reviewersToResume[0]);
            }

            private IActivity ResumeReviewer(ChannelAccount reviewer)
            {
                try
                {
                    ReviewService.ResumeReviewer(reviewer.Id);
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Welcome back ").AppendMention(reviewer).AppendText("! Great to see you doing reviews again.");
                }
                catch (ReviewerNotRegisteredException)
                {
                    return CreateSorryReviewerNotRegisteredReply(TurnContext.Activity.From, reviewer);
                }
                catch (ReviewerNotSuspendedCannotBeResumedException)
                {
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Sorry ").AppendMention(TurnContext.Activity.From).AppendText(", but ").AppendMention(reviewer).AppendText(" is not suspended.");
                }
            }

            private IActivity SelfResume()
            {
                try
                {
                    ReviewService.ResumeReviewer(TurnContext.Activity.From.Id);
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Welcome back ").AppendMention(TurnContext.Activity.From).AppendText("! Great to see you doing reviews again.");
                }
                catch (ReviewerNotRegisteredException)
                {
                    return CreateSorryYouAreNotRegisteredReply(TurnContext.Activity.From);
                }
                catch (ReviewerNotSuspendedCannotBeResumedException)
                {
                    var reply = TurnContext.Activity.CreateReply();
                    return reply.AppendText("Sorry ").AppendMention(TurnContext.Activity.From).AppendText(", but you are not suspended.");
                }
            }
        }
    }
}