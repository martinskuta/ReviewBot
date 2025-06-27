#region copyright

// Copyright 2007 - 2020 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

#region using

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
    public class CanApprovePullRequestsCommand : ReviewCommand
    {
        public CanApprovePullRequestsCommand(IReviewContextStore contextStore)
            : base(contextStore)
        {
        }

        public override double GetMatchingScore(IActivity activity)
        {
            var messageActivity = activity.AsMessageActivity();
            if (messageActivity == null) return 0;

            if (!messageActivity.StartsWithRecipientMention()) return 0;

            var mentions = messageActivity.GetUniqueMentionsExceptRecipient();

            if (mentions.Count != 1) return 0;

            var message = messageActivity.StripMentionText(new[] { messageActivity.Recipient.Id, mentions[0].Mentioned.Id }).StripNewLineAndTrim();

            return message.ContainsAnyOrdinal("can approve pull request", "cannot approve pull requests") ? 1 : 0;
        }

        public override string[] PrintUsages(string myName)
        {
            return new[] { $"@{myName} @reviewer can approve pull requests!", $"@{myName} @reviewer cannot approve pull requests." };
        }

        public override string Name()
        {
            return "Can approve pull requests";
        }

        public override string Description()
        {
            return
                "Allows you to specify if given reviewer can or cannot approve pull requests. If a reviewer that cannot approve pull request is chosen a second one that can will be selected too.";
        }

        protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
        {
            return new CanApprovePullRequestsCommandExecutable(this, turnContext, contextStore);
        }

        private class CanApprovePullRequestsCommandExecutable : ReviewCommandExecutable
        {
            public CanApprovePullRequestsCommandExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
                : base(command, turnContext, contextStore)
            {
            }

            protected override bool IsReadonly => false;

            protected override IActivity ExecuteReviewAction()
            {
                var messageActivity = TurnContext.Activity.AsMessageActivity();
                var reviewerToSet = messageActivity.GetUniqueMentionsExceptRecipient().First().Mentioned;

                if (messageActivity.StripRecipientMention().ContainsAnyOrdinal("can approve pull request"))
                {
                    try
                    {
                        ReviewService.SetCanApprovePullRequestFlag(reviewerToSet.Id, true);
                        return TurnContext.Activity.CreateReply().AppendText("Wow, that's awesome! Congratulations to your promotion ").AppendMention(reviewerToSet)
                                          .AppendText("! :)");
                    }
                    catch (ReviewerNotRegisteredException)
                    {
                        return CreateSorryReviewerNotRegisteredReply(reviewerToSet);
                    }
                }
                else
                {
                    try
                    {
                        ReviewService.SetCanApprovePullRequestFlag(reviewerToSet.Id, false);
                        return TurnContext.Activity.CreateReply().AppendText(
                            "Ok. Btw, did you know that reviewers in alltime and status command with star after their name are the ones who cannot approve pull requests?");
                    }
                    catch (ReviewerNotRegisteredException)
                    {
                        return CreateSorryReviewerNotRegisteredReply(reviewerToSet);
                    }
                }
            }
        }
    }
}