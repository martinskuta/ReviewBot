#region using

using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands;

public abstract class CommandExecutable
{
    private readonly Command _command;
    protected readonly ITurnContext TurnContext;

    protected CommandExecutable(Command command, ITurnContext turnContext)
    {
        _command = command;
        TurnContext = turnContext;
    }

    public abstract Task Execute();

    public abstract IMessageActivity GetReply();

    protected IMessageActivity CreateHelpReply()
    {
        var reply = TurnContext.Activity.CreateReply($"Correct usage of **{_command.Name()}** command:").AppendNewline();

        foreach (var usage in _command.PrintUsages(TurnContext.Activity.Recipient.Name))
        {
            reply.AppendIndentation()
                 .AppendText($"- {usage}")
                 .AppendNewline();
        }

        return reply;
    }
}