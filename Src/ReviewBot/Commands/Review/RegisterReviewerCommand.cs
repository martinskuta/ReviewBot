#region using

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Services.Exceptions;
using Review.Core.Utility;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands.Review;

public class RegisterReviewerCommand : ReviewCommand
{
    public RegisterReviewerCommand(IReviewContextStore contextStore)
        : base(contextStore)
    {
    }

    public override double GetMatchingScore(IActivity activity)
    {
        var messageActivity = activity.AsMessageActivity();
        if (messageActivity == null) return 0;

        if (!messageActivity.StartsWithRecipientMention()) return 0;

        var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
        var mentions = messageActivity.GetUniqueMentionsExceptRecipient();

        if (message.StartsWith("register me", StringComparison.InvariantCultureIgnoreCase) && mentions.IsEmpty()) return 1;

        return message.StartsWith("register", StringComparison.InvariantCultureIgnoreCase) && !mentions.IsEmpty() ? 1 : 0;
    }

    public override string[] PrintUsages(string myName)
    {
        return new[]
        {
            $"@{myName} register @reviewer1, @reviewer2",
            $"@{myName} register me"
        };
    }

    public override string Name()
    {
        return "Register reviewers";
    }

    public override string Description()
    {
        return "Use this command to register member(s) of a channel as a reviewer(s) in the current channel. You can register yourself too.";
    }

    protected override ReviewCommandExecutable CreateReviewExecutable(ITurnContext turnContext, IReviewContextStore reviewContextStore)
    {
        return new RegisterReviewerExecutable(this, turnContext, reviewContextStore);
    }

    private class RegisterReviewerExecutable : ReviewCommandExecutable
    {
        public RegisterReviewerExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
            : base(command, turnContext, contextStore)
        {
        }

        protected override bool IsReadonly => false;

        protected override IActivity ExecuteReviewAction()
        {
            var messageActivity = TurnContext.Activity.AsMessageActivity();
            var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
            var reviewersToRegister =
                messageActivity.GetUniqueMentionsExceptRecipient().Select(m => m.Mentioned).ToList();

            if (reviewersToRegister.IsEmpty())
            {
                if (message == "register me")
                {
                    return RegisterSenderAsReviewer();
                }

                return CreateHelpReply();
            }

            if (reviewersToRegister.Count == 1)
            {
                return RegisterReviewer(reviewersToRegister.First());
            }

            return RegisterReviewers(reviewersToRegister);
        }

        private IActivity RegisterSenderAsReviewer()
        {
            var reviewer = TurnContext.Activity.From;

            try
            {
                ReviewService.RegisterReviewer(reviewer.Id, reviewer.Name);
                return TurnContext.Activity.CreateReply("You are now registered as reviewer.");
            }
            catch (ReviewerAlreadyRegisteredException)
            {
                return TurnContext.Activity.CreateReply("You are already registered.");
            }
        }

        private IActivity RegisterReviewer(ChannelAccount reviewer)
        {
            try
            {
                ReviewService.RegisterReviewer(reviewer.Id, reviewer.Name);
                var reply = TurnContext.Activity.CreateReply().AsMessageActivity();
                reply.AppendMention(reviewer);
                reply.AppendText(" is now registered as reviewer.");
                return reply;
            }
            catch (ReviewerAlreadyRegisteredException)
            {
                var reply = TurnContext.Activity.CreateReply().AsMessageActivity();
                reply.AppendMention(reviewer);
                reply.AppendText(" is already registered.");
                return reply;
            }
        }

        private IActivity RegisterReviewers(IEnumerable<ChannelAccount> reviewers)
        {
            var registered = new List<ChannelAccount>();
            var alreadyRegistered = new List<ChannelAccount>();

            foreach (var reviewer in reviewers)
            {
                try
                {
                    ReviewService.RegisterReviewer(reviewer.Id, reviewer.Name);
                    registered.Add(reviewer);
                }
                catch (ReviewerAlreadyRegisteredException)
                {
                    alreadyRegistered.Add(reviewer);
                }
            }

            var reply = TurnContext.Activity.CreateReply().AsMessageActivity();

            if (!registered.IsEmpty())
            {
                if (registered.Count == 1)
                {
                    reply.AppendMention(registered[0]);
                    reply.AppendText(" is now registered as reviewer.");
                }
                else
                {
                    foreach (var reviewer in registered)
                    {
                        reply.AppendMention(reviewer);
                        reply.AppendText(" ");
                    }

                    reply.AppendText("are now registered as reviewers.");
                }
            }

            if (!alreadyRegistered.IsEmpty())
            {
                reply.AppendText(" ");

                if (alreadyRegistered.Count == 1)
                {
                    reply.AppendMention(alreadyRegistered[0]);
                    reply.AppendText(" was already registered.");
                }
                else
                {
                    foreach (var reviewer in alreadyRegistered)
                    {
                        reply.AppendMention(reviewer);
                        reply.AppendText(" ");
                    }

                    reply.AppendText("were already registered.");
                }
            }

            return reply;
        }
    }
}