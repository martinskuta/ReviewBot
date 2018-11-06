#region using

using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

#endregion

namespace ReviewBot.Commands
{
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

        public abstract IActivity GetReply();

        protected IActivity CreateHelpReply()
        {
            return TurnContext.Activity.CreateReply(_command.PrintUsage(TurnContext.Activity.Recipient.Name));
        }
    }
}