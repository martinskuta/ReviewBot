#region copyright

// Copyright 2007 - 2025 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

#region using

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ReviewBot.Commands;
using ReviewBot.Commands.Review;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot;

public class ReviewBot : IBot
{
    private readonly ILogger<ReviewBot> _logger;
    private readonly IList<Command> _reviewCommands = new List<Command>();

    public ReviewBot(ILoggerFactory loggerFactory, IReviewContextStore contextStore)
    {
        _logger = loggerFactory.CreateLogger<ReviewBot>();
        _logger.LogTrace("Review bot started.");

        _reviewCommands.Add(new RegisterReviewerCommand(contextStore));
        _reviewCommands.Add(new CurrentStatusCommand(contextStore));
        _reviewCommands.Add(new AllTimeCommand(contextStore));
        _reviewCommands.Add(new FindReviewerCommand(contextStore));
        _reviewCommands.Add(new AddReviewCommand(contextStore));
        _reviewCommands.Add(new RemoveReviewCommand(contextStore));
        _reviewCommands.Add(new SuspendReviewerCommand(contextStore));
        _reviewCommands.Add(new MakeReviewerBusyCommand(contextStore));
        _reviewCommands.Add(new MakeAvailableCommand(contextStore));
        _reviewCommands.Add(new CanApprovePullRequestsCommand(contextStore));
        _reviewCommands.Add(new FixIdCommand(contextStore));
        _reviewCommands.Add(new HelpCommand(_reviewCommands));
    }

    public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new CancellationToken())
    {
        _logger.LogTrace($"Received message: {turnContext.Activity.Text}. Activity object: {JsonConvert.SerializeObject(turnContext.Activity, Formatting.Indented)}");

        if (turnContext.Activity.Type == ActivityTypes.Message)
        {
            // In case we receive all channel messages, we wanna react only to messages addressed to bot currently
            if (!turnContext.Activity.IsPrivateChat() && !turnContext.Activity.IsRecipientMentioned())
            {
                _logger.LogTrace("Message not addressed to review bot. Ignoring.");
                return;
            }

            await HandleMessage(turnContext);
            return;
        }

        _logger.LogTrace($"Activity not handled: {turnContext.Activity.Type}");
    }

    private async Task HandleMessage(ITurnContext turnContext)
    {
        var matchingCommand = GetMatchingCommand(turnContext);
        if (matchingCommand == null)
        {
            _logger.LogTrace($"Message couldn't be handled. Returning help. Message: {turnContext.Activity.Text}");
            await ReplyWithHelp(turnContext);
            return;
        }

        await ExecuteCommand(turnContext, matchingCommand);
    }

    private async Task ExecuteCommand(ITurnContext turnContext, Command command)
    {
        _logger.LogTrace($"Message will be handled by {command.GetType().Name}. Message: {turnContext.Activity.Text}");

        turnContext.Activity.ReplyInSlackThread();

        var commandExecutable = command.CreateExecutable(turnContext);

        await commandExecutable.Execute();

        var messageActivity = commandExecutable.GetReply();

        messageActivity.TextFormat = TextFormatTypes.Markdown;

        await turnContext.SendActivityAsync(messageActivity);
    }

    private Task ReplyWithHelp(ITurnContext turnContext)
    {
        return turnContext.SendActivityAsync($"Sorry, I didn't understand your message. Use '@{turnContext.Activity.Recipient.Name} help' to see what I understand.");
    }

    private Command GetMatchingCommand(ITurnContext turnContext)
    {
        Command matchingCommand = null;
        var highestMatchingScore = 0.0;

        foreach (var reviewCommand in _reviewCommands)
        {
            var commandMatchingScore = reviewCommand.GetMatchingScore(turnContext.Activity);
            if (commandMatchingScore > 0 && commandMatchingScore > highestMatchingScore)
            {
                matchingCommand = reviewCommand;
                highestMatchingScore = commandMatchingScore;
            }
        }

        return matchingCommand;
    }
}