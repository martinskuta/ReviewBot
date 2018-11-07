#region using

using System;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Services;
using Review.Core.Services.Exceptions;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands.Review
{
    public class AddReviewCommand : ReviewCommand
    {
        public AddReviewCommand(IReviewContextStore contextStore)
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

            var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
            return message.StartsWith("add  to", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
        }

        public override string PrintUsage(string myName)
        {
            return $"Add review: Add @{myName} to @reviewer1 and @reviewer2";
        }

        protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
        {
            return new AddReviewExecutable(this, turnContext, contextStore);
        }

        private class AddReviewExecutable : ReviewCommandExecutable
        {
            public AddReviewExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
                : base(command, turnContext, contextStore)
            {
            }

            protected override bool IsReadonly => false;

            protected override IActivity Execute(IReviewService reviewService)
            {
                var featureAuthor = TurnContext.Activity.From;
                var messageActivity = TurnContext.Activity.AsMessageActivity();
                var reviewersToRegister = messageActivity.GetUniqueMentionsExceptRecipient().Select(m => m.Mentioned).ToList();

                try
                {
                    reviewService.AddReview(reviewersToRegister.Select(r => r.Id).ToArray());
                    return TurnContext.Activity.CreateReply("Added.");
                }
                catch (NoReviewerAvailableException)
                {
                    return TurnContext.Activity.CreateReply("Sorry ").AppendMention(featureAuthor).AppendText(", there are no reviewers registered.");
                }
                catch (ReviewerNotRegisteredException e)
                {
                    return TurnContext.Activity.CreateReply("Sorry ")
                                      .AppendMention(featureAuthor)
                                      .AppendText(", but ")
                                      .AppendMention(reviewersToRegister.First(r => r.Id == e.ReviewerId))
                                      .AppendText(" is not registered as reviewer.");
                }
            }
        }
    }
}