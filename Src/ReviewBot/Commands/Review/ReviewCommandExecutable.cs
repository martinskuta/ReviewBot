#region using

using System;
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
        private ILogger _logger;
        private IActivity _reply;
        protected IReviewService ReviewService;

        protected ReviewCommandExecutable(Command command, ITurnContext turnContext, IReviewContextStore contextStore)
            : base(command, turnContext)
        {
            _contextStore = contextStore;
        }

        /// <summary>
        ///     If true, then the executable is only reading from review context and doesn't need to save any changes.
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

        protected IActivity CreateSorryYouAreNotRegisteredReply()
        {
            return TurnContext.Activity.CreateReply("Sorry ")
                              .AppendMention(TurnContext.Activity.From)
                              .AppendText(", but you are not registered as reviewer.");
        }

        protected IActivity CreateSorryReviewerNotRegisteredReply(ChannelAccount notRegistered)
        {
            return TurnContext.Activity.CreateReply("Sorry ")
                              .AppendMention(TurnContext.Activity.From)
                              .AppendText(", but ")
                              .AppendMention(notRegistered)
                              .AppendText(" is not registered as reviewer.");
        }

        protected IActivity CreateSorryNoReviewersRegisteredYetReply()
        {
            return TurnContext.Activity.CreateReply("Sorry ")
                              .AppendMention(TurnContext.Activity.From)
                              .AppendText(", there are no reviewers registered here yet.");
        }

        private string GetReviewContextId()
        {
            if (TurnContext.Activity.IsMsTeamsActivity())
            {
                var tenantId = TurnContext.Activity.GetMsTeamsTenantId();
                var channelId = TurnContext.Activity.GetMsTeamsChannelId();

                return HttpUtility.UrlDecode($"{tenantId}_{channelId}");
            }

            if (TurnContext.Activity.IsSlackActivity())
            {
                var teamId = TurnContext.Activity.GetSlackTeamId();
                var channelId = TurnContext.Activity.GetSlackChannelId();

                return HttpUtility.UrlDecode($"{teamId}_{channelId}");
            }

            throw new Exception($"Unsupported channel: {TurnContext.Activity.ChannelId}");
        }
    }