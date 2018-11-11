#region using

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.DataModel;
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
            var otherMentions = messageActivity.GetUniqueMentionsExceptRecipient();

            //SKYE-123 is ready for @review
            if (message.Contains($"is ready for {recipientMention.Text}", StringComparison.InvariantCultureIgnoreCase)) return 1;

            //@reviewer is looking for @review of SKYE-1234
            if (message.Contains($"is looking for {recipientMention.Text} of"))
            {
                var beforeIsLookingFor = message.Substring(0, message.IndexOf("is looking for"));

                if (otherMentions.Count(m => beforeIsLookingFor.Contains(m.Text)) == 1) return 1;
            }

            //ma and @reviewer are looking for @review of SKYE-1234
            //@reviewer1 @reviewer2 and @reviewer3 are looking for @review of SKYE-1234
            if (otherMentions.Count >= 1 && message.Contains($"are looking for {recipientMention.Text} of"))
            {
                var beforeAreLookingFor = message.Substring(0, message.IndexOf("are looking for"));

                if (beforeAreLookingFor.Contains(" me ") && otherMentions.Count(m => beforeAreLookingFor.Contains(m.Text)) >= 1) return 1;
                if (otherMentions.Count(m => beforeAreLookingFor.Contains(m.Text)) >= 2) return 1;
            }

            return 0;
        }

        public override string PrintUsage(string myName)
        {
            return
                $"Find reviewer: 'SKYE-1234 is ready for @{myName}' or '@reviewer is looking for @{myName} of SKYE-1234' or '@reviewer1, @reviewer2 and me are looking for @{myName} of SKYE-1234'";
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
                var messageActivity = TurnContext.Activity.AsMessageActivity();
                var otherMentions = TurnContext.Activity.GetUniqueMentionsExceptRecipient();

                if (messageActivity.Text.Contains("is ready for")) return SenderIsLookingForReview();

                if (messageActivity.Text.Contains("is looking for"))
                {
                    var beforeIsLookingFor = messageActivity.Text.Substring(0, messageActivity.Text.IndexOf("is looking for"));

                    var developer = otherMentions.First(m => beforeIsLookingFor.Contains(m.Text)).Mentioned;
                    return OtherDeveloperIsLookingForReview(developer);
                }

                var beforeAreLookingFor = messageActivity.Text.Substring(0, messageActivity.Text.IndexOf("are looking for"));
                var developers = otherMentions.Where(m => beforeAreLookingFor.Contains(m.Text)).Select(m => m.Mentioned).ToList();
                if (beforeAreLookingFor.Contains(" me ")) developers.Add(TurnContext.Activity.From);

                return OtherDevelopersAreLookingForReview(developers);
            }

            private IActivity SenderIsLookingForReview()
            {
                var featureAuthor = TurnContext.Activity.From;

                try
                {
                    var assignedReviewer = ReviewService.AddReviewToHighestDebtor(new[] { featureAuthor.Id });
                    return CreateAssignTheReviewToReply(assignedReviewer);
                }
                catch (NoReviewerAvailableException)
                {
                    return CreateSorryNoReviewersAvailable();
                }
            }

            private IActivity OtherDeveloperIsLookingForReview(ChannelAccount developer)
            {
                try
                {
                    var assignedReviewer = ReviewService.AddReviewToHighestDebtor(new[] { developer.Id });
                    return CreateAssignTheReviewToReply(assignedReviewer);
                }
                catch (NoReviewerAvailableException)
                {
                    return CreateSorryNoReviewersAvailable();
                }
            }

            private IActivity OtherDevelopersAreLookingForReview(IList<ChannelAccount> developers)
            {
                try
                {
                    var assignedReviewer = ReviewService.AddReviewToHighestDebtor(developers.Select(d => d.Id).ToArray());
                    return CreateAssignTheReviewToReply(assignedReviewer);
                }
                catch (NoReviewerAvailableException)
                {
                    return CreateSorryNoReviewersAvailable();
                }
            }

            private IActivity CreateSorryNoReviewersAvailable()
            {
                return TurnContext.Activity.CreateReply("Sorry ").AppendMention(TurnContext.Activity.From).AppendText(", there are no reviewers available.");
            }

            private IActivity CreateAssignTheReviewToReply(Reviewer reviewer)
            {
                return TurnContext.Activity.CreateReply()
                                  .AppendMention(TurnContext.Activity.From)
                                  .AppendText(" assign the review to ")
                                  .AppendMention(new ChannelAccount(reviewer.Id, reviewer.Name));
            }
        }
    }
}