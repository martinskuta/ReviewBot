#region using

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Services.Exceptions;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands.Review
{
    public class FindReviewerCommand : ReviewCommand
    {
        public FindReviewerCommand(IReviewContextStore contextStore)
            : base(contextStore)
        {
        }

        public override double GetMatchingScore(IActivity activity)
        {
            var messageActivity = activity.AsMessageActivity();

            var recipientMention = messageActivity?.GetRecipientMention();
            if (recipientMention == null) return 0;

            var message = messageActivity.Text.StripNewLineAndTrim();
            return message.Contains($"is ready for {recipientMention.Text}", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
        }

        public override string PrintUsage(string myName)
        {
            return $"Find reviewer: SKYE-1234 is ready for @{myName}";
        }

        protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
        {
            return new FindReviewerExecutable(this, turnContext, contextStore);
        }

        private class FindReviewerExecutable : ReviewCommandExecutable
        {
            public FindReviewerExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
                : base(command, turnContext, contextStore)
            {
            }

            protected override bool IsReadonly => false;

            protected override IActivity ExecuteReviewAction()
            {
                var featureAuthor = TurnContext.Activity.From;

                try
                {
                    var reviewer = ReviewService.AddReviewToHighestDebtor(new[] { featureAuthor.Id });
                    return TurnContext.Activity.CreateReply()
                                      .AppendMention(featureAuthor)
                                      .AppendText(" assign the review to ")
                                      .AppendMention(new ChannelAccount(reviewer.Id, reviewer.Name));
                }
                catch (NoReviewerAvailableException)
                {
                    return TurnContext.Activity.CreateReply("Sorry ").AppendMention(featureAuthor).AppendText(", there are no reviewers available.");
                }
            }
        }
    }
}