#region using

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.DataModel;
using Review.Core.Services.Exceptions;
using Review.Core.Utility;
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
            if (message.ContainsAnyOrdinal($"is ready for {recipientMention.Text}")) return 1;

            //@reviewer is looking for @review of SKYE-1234
            if (message.ContainsAnyOrdinal($"is looking for {recipientMention.Text} of"))
            {
                var wordsBeforeIsLookingFor = messageActivity.GetWordsBeforePhrase("is looking for");

                if (otherMentions.Count(m => wordsBeforeIsLookingFor.ContainsAnyOrdinal(m.Text)) == 1) return 1;
            }

            //ma and @reviewer are looking for @review of SKYE-1234
            //@reviewer1 @reviewer2 and @reviewer3 are looking for @review of SKYE-1234
            if (otherMentions.Count >= 1 && message.ContainsAnyOrdinal($"are looking for {recipientMention.Text} of"))
            {
                var wordsBeforeAreLookingFor = messageActivity.GetWordsBeforePhrase("are looking for");

                if (wordsBeforeAreLookingFor.ContainsAnyOrdinal("me", "I") && otherMentions.Count(m => wordsBeforeAreLookingFor.ContainsAnyOrdinal(m.Text)) >= 1) return 1;
                if (otherMentions.Count(m => wordsBeforeAreLookingFor.ContainsAnyOrdinal(m.Text)) >= 2) return 1;
            }

            return 0;
        }

        public override string[] PrintUsages(string myName)
        {
            return new[]
            {
                $"SKYE-1234 is ready for @{myName}",
                $"@reviewer is looking for @{myName} of SKYE-1234",
                $"@reviewer1, @reviewer2 and me are looking for @{myName} of SKYE-1234"
            };
        }

        public override string Name()
        {
            return "Find reviewer";
        }

        public override string Description()
        {
            return
                "Automatic way of looking for a reviewer with the highest debt. If there are two or more reviewers with highest debt, then out of those one is randomly chosen. There is also way of asking for review of feature that you did not implement, eg. ask for someone else. Also you can exclude multiple reviewers if they were working on the feature.";
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

                if (messageActivity.Text.ContainsAnyOrdinal("is ready for")) return SenderIsLookingForReview();

                if (messageActivity.Text.ContainsAnyOrdinal("is looking for"))
                {
                    var wordsBeforeIsLookingFor = messageActivity.GetWordsBeforePhrase("is looking for");

                    var developer = otherMentions.First(m => wordsBeforeIsLookingFor.ContainsAnyOrdinal(m.Text)).Mentioned;
                    return OtherDeveloperIsLookingForReview(developer);
                }

                var wordsBeforeAreLookingFor = messageActivity.GetWordsBeforePhrase("are looking for");

                var developers = otherMentions.Where(m => wordsBeforeAreLookingFor.ContainsAnyOrdinal(m.Text)).Select(m => m.Mentioned).ToList();
                if (wordsBeforeAreLookingFor.ContainsAnyOrdinal("me", "I")) developers.Add(TurnContext.Activity.From);

                return OtherDevelopersAreLookingForReview(developers);
            }

            private IActivity SenderIsLookingForReview()
            {
                var featureAuthor = TurnContext.Activity.From;

                try
                {
                    var assignedReviewers = ReviewService.AddReviewToHighestDebtor(new[] { featureAuthor.Id });
                    return CreateAssignTheReviewToReply(assignedReviewers);
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
                    var assignedReviewers = ReviewService.AddReviewToHighestDebtor(new[] { developer.Id });
                    return CreateAssignTheReviewToReply(assignedReviewers);
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
                    var assignedReviewers = ReviewService.AddReviewToHighestDebtor(developers.Select(d => d.Id).ToArray());
                    return CreateAssignTheReviewToReply(assignedReviewers);
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

            private IActivity CreateAssignTheReviewToReply(IReadOnlyList<Reviewer> reviewers)
            {
                if (reviewers.Count == 1)
                {
                    var reviewer = reviewers[0];
                    if (reviewer.CanApprovePullRequest)
                    {
                        return TurnContext.Activity.CreateReply()
                                          .AppendMention(TurnContext.Activity.From)
                                          .AppendText(" assign the review to ")
                                          .AppendMention(new ChannelAccount(reviewer.Id, reviewer.Name))
                                          .AppendText(" and don't forget to create pull request!");
                    }

                    return TurnContext.Activity.CreateReply()
                                      .AppendMention(TurnContext.Activity.From)
                                      .AppendText(" assign the review to ")
                                      .AppendMention(new ChannelAccount(reviewer.Id, reviewer.Name))
                                      .AppendText(" and don't forget to create pull request! PS: Someone needs to review it again after ")
                                      .AppendMention(new ChannelAccount(reviewer.Id, reviewer.Name))
                                      .AppendText(", because she cannot approve pull requests.");
                }

                var juniorReviewer = reviewers.Single(r => !r.CanApprovePullRequest);
                var seniorReviewer = reviewers.Single(r => r.CanApprovePullRequest);

                return TurnContext.Activity.CreateReply()
                                  .AppendMention(TurnContext.Activity.From)
                                  .AppendText(" assign the review to ")
                                  .AppendMention(new ChannelAccount(juniorReviewer.Id, juniorReviewer.Name))
                                  .AppendText(" and then for final review to ")
                                  .AppendMention(new ChannelAccount(seniorReviewer.Id, seniorReviewer.Name))
                                  .AppendText(". Also don't forget to create pull request!");
            }
        }
    }
}