#region copyright

// Copyright 2007 - 2019 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

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
    public class AllTimeCommand : ReviewCommand
    {
        public AllTimeCommand(IReviewContextStore contextStore)
            : base(contextStore)
        {
        }

        public override double GetMatchingScore(IActivity activity)
        {
            var messageActivity = activity.AsMessageActivity();
            if (messageActivity == null) return 0;

            if (!messageActivity.StartsWithRecipientMention()) return 0;

            var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
            return message.Equals("alltime", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
        }

        public override string[] PrintUsages(string myName)
        {
            return new[] { $"@{myName} alltime" };
        }

        public override string Name()
        {
            return "All time statistics";
        }

        public override string Description()
        {
            return "Shows stats like total number of reviews for all reviewers, including inactive ones.";
        }

        protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
        {
            return new AllTimeCommandExecutable(this, turnContext, contextStore);
        }

        private class AllTimeCommandExecutable : ReviewCommandExecutable
        {
            public AllTimeCommandExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
                : base(command, turnContext, contextStore)
            {
            }

            protected override bool IsReadonly => true;

            protected override IActivity ExecuteReviewAction()
            {
                var activeReviewers = ReviewService.GetAllReviewers()
                                                   .OrderByDescending(r => r.ReviewCount)
                                                   .ThenBy(r => r.Name)
                                                   .ToList();

                if (activeReviewers.IsEmpty())
                {
                    return TurnContext.Activity.CreateReply("There are no registered reviewers.");
                }

                var reply = TurnContext.Activity.CreateReply("Ordered by review count:").AppendNewline();
                foreach (var reviewer in activeReviewers)
                {
                    reply.AppendText(
                             $"**{reviewer.Name}**{(reviewer.CanApprovePullRequest ? "" : "*")} ({reviewer.Status}): Reviews: {reviewer.ReviewCount}, Debt: {reviewer.ReviewDebt}")
                         .AppendNewline();
                }

                return reply;
            }
        }
    }
}