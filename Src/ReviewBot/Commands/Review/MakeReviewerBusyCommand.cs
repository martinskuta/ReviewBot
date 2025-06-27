#region using

using System;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Review.Core.Services.Exceptions;
using Review.Core.Utility;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands.Review;

public class MakeReviewerBusyCommand : ReviewCommand
{
    private readonly ILoggerFactory _loggerFactory;

    public MakeReviewerBusyCommand(ILoggerFactory loggerFactory, IReviewContextStore contextStore)
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
        var mentions = messageActivity.GetUniqueMentionsExceptRecipient();

        if (mentions.IsEmpty() && message.Equals("I am busy", StringComparison.InvariantCultureIgnoreCase)) return 1;

        return mentions.Count == 1 && message.EndsWith("is busy", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
    }

    public override string[] PrintUsages(string myName)
    {
        return new[]
        {
            $"@{myName} @reviewer is busy",
            $"@{myName} I am busy"
        };
    }

    public override string Name()
    {
        return "Make reviewer busy";
    }

    public override string Description()
    {
        return "Way to change status of a reviewer to busy. Busy reviewers are NOT considered when looking for a reviewer, but their debt increases with every review they skip.";
    }

    protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore contextStore)
    {
        return new MakeReviewerBusyExecutable(_loggerFactory, this, turnContext, contextStore);
    }

    private class MakeReviewerBusyExecutable : ReviewCommandExecutable
    {
        public MakeReviewerBusyExecutable(ILoggerFactory loggerFactory, Command command, ITurnContext turnContext, IReviewContextStore contextStore)
            : base(loggerFactory, command, turnContext, contextStore)
        {
        }

        protected override bool IsReadonly => false;

        protected override IActivity ExecuteReviewAction()
        {
            var messageActivity = TurnContext.Activity.AsMessageActivity();
            var reviewerToMakeBusy = messageActivity.GetUniqueMentionsExceptRecipient().FirstOrDefault()?.Mentioned;

            return reviewerToMakeBusy == null ? MakeSelfBusy() : MakeReviewerBusy(reviewerToMakeBusy);
        }

        private IActivity MakeReviewerBusy(ChannelAccount reviewer)
        {
            try
            {
                ReviewService.MakeReviewerBusy(reviewer.Id);
                var reply = TurnContext.Activity.CreateReply();
                return reply.AppendText("Ok ").AppendMention(TurnContext.Activity.From).AppendText(". I will not assign ").AppendMention(reviewer).AppendText(" any reviews.");
            }
            catch (ReviewerNotRegisteredException)
            {
                return CreateSorryReviewerNotRegisteredReply(reviewer);
            }
            catch (ReviewerAlreadyBusyException)
            {
                var reply = TurnContext.Activity.CreateReply();
                return reply.AppendMention(TurnContext.Activity.From).AppendText(", I know that already. ").AppendMention(reviewer).AppendText(" must be really busy!");
            }
            catch (ReviewerSuspendedCannotBeBusyException)
            {
                var reply = TurnContext.Activity.CreateReply();
                return reply.AppendMention(TurnContext.Activity.From)
                            .AppendText(", to my knowledge, ")
                            .AppendMention(reviewer)
                            .AppendText(" is having time off! So I hope ")
                            .AppendMention(reviewer)
                            .AppendText(" is not busy.");
            }
        }

        private IActivity MakeSelfBusy()
        {
            try
            {
                ReviewService.MakeReviewerBusy(TurnContext.Activity.From.Id);
                var reply = TurnContext.Activity.CreateReply();
                return reply.AppendText("Ok ").AppendMention(TurnContext.Activity.From).AppendText(". I will not assign you any reviews, so you can get things done.");
            }
            catch (ReviewerNotRegisteredException)
            {
                return CreateSorryYouAreNotRegisteredReply();
            }
            catch (ReviewerAlreadyBusyException)
            {
                var reply = TurnContext.Activity.CreateReply();
                return reply.AppendMention(TurnContext.Activity.From).AppendText(", I know that already. Stop lurking around here and get your things done!");
            }
            catch (ReviewerSuspendedCannotBeBusyException)
            {
                var reply = TurnContext.Activity.CreateReply();
                return reply.AppendMention(TurnContext.Activity.From)
                            .AppendText(", to my knowledge, you are having time off! Resume yourself first if you are back from your time off.");
            }
        }
    }
}