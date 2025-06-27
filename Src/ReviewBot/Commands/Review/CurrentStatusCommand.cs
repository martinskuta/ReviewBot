#region using

using System;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Review.Core.Utility;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands.Review;

public class CurrentStatusCommand : ReviewCommand
{
    private readonly ILoggerFactory _loggerFactory;

    public CurrentStatusCommand(ILoggerFactory loggerFactory, IReviewContextStore contextStore)
        : base(contextStore)
    {
        _loggerFactory = loggerFactory;
    }

    public override double GetMatchingScore(IActivity activity)
    {
        var messageActivity = activity.AsMessageActivity();
        if (messageActivity == null) return 0;

        if (!messageActivity.StartsWithRecipientMention()) return 0;

        var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
        return message.Equals("status", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
    }

    public override string[] PrintUsages(string myName)
    {
        return new[] { $"@{myName} status" };
    }

    public override string Name()
    {
        return "Current status";
    }

    public override string Description()
    {
        return "Shows debt of currently active reviewers";
    }

    protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
    {
        return new CurrentStatusExecutable(_loggerFactory, this, turnContext, contextStore);
    }

    private class CurrentStatusExecutable : ReviewCommandExecutable
    {
        public CurrentStatusExecutable(ILoggerFactory loggerFactory, Command command, ITurnContext turnContext, IReviewContextStore contextStore)
            : base(loggerFactory, command, turnContext, contextStore)
        {
        }

        protected override bool IsReadonly => true;

        protected override IActivity ExecuteReviewAction()
        {
            var activeReviewers = ReviewService.GetAllReviewers()
                                               .Where(r => !r.IsSuspended)
                                               .OrderByDescending(r => r.ReviewDebt)
                                               .ThenBy(r => r.Name)
                                               .ToList();

            if (activeReviewers.IsEmpty())
            {
                return TurnContext.Activity.CreateReply("There are no active reviewers.");
            }

            var reply = TurnContext.Activity.CreateReply("Ordered by debt:").AppendNewline();
            foreach (var reviewer in activeReviewers)
            {
                reply.AppendText($"**{reviewer.Name}**{(reviewer.CanApprovePullRequest ? "" : "*")} ({reviewer.Status}) Debt: {reviewer.ReviewDebt}")
                     .AppendNewline();
            }

            return reply;
        }
    }
}