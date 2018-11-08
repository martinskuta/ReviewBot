#region using

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

#endregion

namespace ReviewBot.Commands
{
    public abstract class Command
    {
        public abstract double GetMatchingScore(IActivity activity);

        public abstract CommandExecutable CreateExecutable(ITurnContext turnContext);

        public abstract string PrintUsage(string myName);
    }
}