﻿#region copyright

// Copyright 2007 - 2019 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands
{
    public class HelpCommand : Command
    {
        private readonly IList<Command> _allCommands;

        public HelpCommand(IList<Command> allCommands)
        {
            _allCommands = allCommands;
        }

        public override double GetMatchingScore(IActivity activity)
        {
            var messageActivity = activity.AsMessageActivity();
            if (messageActivity == null) return 0;

            if (!messageActivity.StartsWithRecipientMention()) return 0;

            var message = messageActivity.StripRecipientMention().StripNewLineAndTrim();
            return message.Equals("help", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
        }

        public override CommandExecutable CreateExecutable(ITurnContext turnContext)
        {
            return new HelpCommandExecutable(this, turnContext);
        }

        public override string[] PrintUsages(string myName) => new[] { $"@{myName} help" };

        public override string Name()
        {
            return "Help";
        }

        public override string Description()
        {
            return "Shows features of this bot";
        }

        private class HelpCommandExecutable : CommandExecutable
        {
            private readonly HelpCommand _command;
            private Activity _reply;

            public HelpCommandExecutable(HelpCommand command, ITurnContext turnContext)
                : base(command, turnContext)
            {
                _command = command;
            }

            public override Task Execute()
            {
                _reply = TurnContext.Activity.CreateReply("I am a bot that helps your channel to distribute reviews equally. The reviews are distributed per channel and every channel has their own statistics, so make sure you are using the correct channel!. The only command that works everywhere, even in private chat, is help command.");
                _reply.AppendNewline();
                _reply.AppendText("This is what I can do for you:");
                _reply.AppendNewline();

                foreach (var reviewCommand in _command._allCommands)
                {
                    _reply.AppendNewline()
                          .AppendText($"*{reviewCommand.Name()}*: {reviewCommand.Description()}")
                          .AppendNewline()
                          .AppendText("Usage:")
                          .AppendNewline();

                    foreach (var usage in reviewCommand.PrintUsages(TurnContext.Activity.Recipient.Name))
                    {
                        _reply.AppendTab()
                              .AppendText($"- {usage}")
                              .AppendNewline();
                    }

                    _reply.AppendNewline();
                }

                return Task.CompletedTask;
            }

            public override IActivity GetReply() => _reply;
        }
    }
}