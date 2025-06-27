#region copyright

// Copyright 2007 - 2019 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

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

namespace ReviewBot.Commands.Review;

public class AddReviewCommand : ReviewCommand
{
    public AddReviewCommand(IReviewContextStore contextStore)
        : base(contextStore)
    {
    }

    public override double GetMatchingScore(IActivity activity)
    {
        var messageActivity = activity.AsMessageActivity();
        if (messageActivity == null) return 0;

        if (messageActivity.GetUniqueMentionsExceptRecipient().IsEmpty()) return 0;

        var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
        return message.StartsWith("add  to", StringComparison.OrdinalIgnoreCase) || message.StartsWith("assign  to", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
    }

    public override string[] PrintUsages(string myName)
    {
        return new[] { $"Add @{myName} to @reviewer1", $"Assign @{myName} to @reviewer1, @reviewer2 and @reviewer3" };
    }

    public override string Name()
    {
        return "Add review";
    }

    public override string Description()
    {
        return "Way to assign review directly to given reviewer(s). Debt is recalculated. On purpose not possible to add review to yourself.";
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

        protected override IActivity ExecuteReviewAction()
        {
            var messageActivity = TurnContext.Activity.AsMessageActivity();
            var reviewersToRegister = messageActivity.GetUniqueMentionsExceptRecipient().Select(m => m.Mentioned).ToList();

            try
            {
                ReviewService.AddReview(reviewersToRegister.Select(r => r.Id).ToArray());
                return TurnContext.Activity.CreateReply("Added.");
            }
            catch (NoReviewerAvailableException)
            {
                return CreateSorryNoReviewersRegisteredYetReply();
            }
            catch (ReviewerNotRegisteredException e)
            {
                return CreateSorryReviewerNotRegisteredReply(reviewersToRegister.First(r => r.Id == e.ReviewerId));
            }
        }
    }
}