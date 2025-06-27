#region copyright

// Copyright 2007 - 2022 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

#region using

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Utility;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands.Review;

public class FixIdCommand : ReviewCommand
{
    public FixIdCommand(IReviewContextStore contextStore)
        : base(contextStore)
    {
    }

    public override double GetMatchingScore(IActivity activity)
    {
        var messageActivity = activity.AsMessageActivity();
        if (messageActivity == null) return 0;

        if (!messageActivity.StartsWithRecipientMention()) return 0;

        var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
        return message.Equals("fix my id", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
    }

    public override string[] PrintUsages(string myName)
    {
        return new[] { $"@{myName} fix my id" };
    }

    public override string Name()
    {
        return "Fix my id";
    }

    public override string Description()
    {
        return
            "When the bot is re-registered on a new tenant then the IDs used to identify users change, so this command tries to update and re-map your current id, with id that is stored in database by matching via user's display name. This is needed only when the bot is re-registered on bot framework and the channel account ids change.";
    }

    protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
    {
        return new FixMyIdExecutable(this, turnContext, contextStore);
    }

    private sealed class FixMyIdExecutable : ReviewCommandExecutable
    {
        public FixMyIdExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
            : base(command, turnContext, contextStore)
        {
        }

        protected override bool IsReadonly => false;

        protected override IMessageActivity ExecuteReviewAction()
        {
            var featureAuthor = TurnContext.Activity.From;

            foreach (var reviewer in ReviewService.GetAllReviewers())
            {
                var currentName = featureAuthor.Name.NormalizeDiacritics();
                var reviewerName = reviewer.Name.NormalizeDiacritics();

                if (!currentName.StartsWith(reviewerName)) continue;

                if (featureAuthor.Id == reviewer.Id)
                    return TurnContext.Activity.CreateReply("Id is already up to date.");

                ReviewService.UpdateId(reviewer.Id, featureAuthor.Id);
                return TurnContext.Activity.CreateReply("Id updated successfully.");
            }

            return TurnContext.Activity.CreateReply("Could not find a reviewer that would match your display name.");
        }
    }
}