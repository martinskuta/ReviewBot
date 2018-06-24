using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;

namespace ReviewBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : Controller
    {
        private readonly IConfiguration configuration;
        public MessagesController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        //[Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            var appCredentials = new MicrosoftAppCredentials(configuration);
            var client = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);
            var reply = activity.CreateReply();
            if (activity.Type == ActivityTypes.Message)
            {
                reply.Text = $"echo: {activity.Text}";
            }
            else
            {
                reply.Text = $"activity type: {activity.Action}";
            }
            await client.Conversations.ReplyToActivityAsync(reply);
            return Ok();
        }

        public string Get()
        {
            return "Hello";
        }
    }
}