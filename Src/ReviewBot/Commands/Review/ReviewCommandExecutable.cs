#region using

using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Review.Core.Services;
using ReviewBot.Storage;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Commands.Review
{
    public abstract class ReviewCommandExecutable : CommandExecutable
    {
        private readonly IReviewContextStore _contextStore;
        private IActivity _reply;
        protected IReviewService ReviewService;

        protected ReviewCommandExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
            : base(command, turnContext)
        {
            _contextStore = contextStore;
        }

        /// <summary>
        ///   If true, then the executable is only reading from review context and doesn't need to save any changes.
        /// </summary>
        protected abstract bool IsReadonly { get; }

        public override async Task Execute()
        {
            var reviewContext = await _contextStore.GetContext(GetReviewContextId());

            ReviewService = new ReviewService(reviewContext);

            _reply = ExecuteReviewAction();

            if (IsReadonly) return;

            await _contextStore.SaveContext(reviewContext);
        }

        public override IActivity GetReply() => _reply;

        protected abstract IActivity ExecuteReviewAction();

        protected IActivity CreateSorryYouAreNotRegisteredReply(ChannelAccount sender)
        {
            return TurnContext.Activity.CreateReply("Sorry ")
                              .AppendMention(sender)
                              .AppendText(", but you are not registered as reviewer.");
        }

        protected IActivity CreateSorryReviewerNotRegisteredReply(ChannelAccount sender, ChannelAccount notRegistered)
        {
            return TurnContext.Activity.CreateReply("Sorry ")
                              .AppendMention(sender)
                              .AppendText(", but ")
                              .AppendMention(notRegistered)
                              .AppendText(" is not registered as reviewer.");
        }

        protected IActivity CreateSorryNoReviewersRegisteredYetReply(ChannelAccount sender)
        {
            return TurnContext.Activity.CreateReply("Sorry ")
                              .AppendMention(sender)
                              .AppendText(", there are no reviewers registered here yet.");
        }

        private string GetReviewContextId()
        {
            var tenantId = TurnContext.Activity.GetMsTeamsTenantId();
            var channelId = TurnContext.Activity.GetMsTeamsChannelId();

            return HttpUtility.UrlDecode($"{tenantId}_{channelId}");
        }
    }
}