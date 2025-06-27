#region using

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

#endregion

namespace ReviewBot.Tests;

/// <summary>
///     Used for test purposes only.
/// </summary>
public class MSTeamsTurnContext : TurnContext
{
    private MSTeamsTurnContext(Activity activity)
        : base(new TestAdapter(), activity)
    {
    }

    /// <summary>
    ///     Gets the queue of responses from the bot.
    /// </summary>
    public Queue<Activity> Responses => ((TestAdapter)Adapter).ActiveQueue;

    /// <summary>
    ///     Creates MS Teams context that represents user sending message to bot in a channel (MS Teams channel).
    /// </summary>
    public static MSTeamsTurnContext CreateUserToBotChannelMessage(string message)
    {
        var activity = new Activity(ActivityTypes.Message)
        {
            Text = message,
            Id = "1541717374898",
            Timestamp = new DateTime(2018, 1, 30, 23, 59, 59).ToUniversalTime(),
            LocalTimestamp = new DateTime(2018, 1, 30, 23, 59, 59).ToLocalTime(),
            ServiceUrl = "https://service.net/emea/",
            ChannelId = "msteams",
            Conversation = new ConversationAccount(true, "channel", "channel_id"),
            From = new ChannelAccount("Sender_id", "Sender"),
            Recipient = new ChannelAccount("Review_id", "Review"),
            Entities = new List<Entity>(),
            TextFormat = "plain"
        };

        dynamic chanelData = new ExpandoObject();
        chanelData.tenant = new ExpandoObject();
        chanelData.channel = new ExpandoObject();
        chanelData.team = new ExpandoObject();
        chanelData.tenant.id = "tenant_id";
        chanelData.channel.id = "channel_id";
        chanelData.team.id = "team_id";
        activity.ChannelData = chanelData;

        var turnContext = new MSTeamsTurnContext(activity);

        ConvertMentions(activity);

        return turnContext;
    }

    /// <summary>
    ///     Creates MS Teams context that represents user sending a message to bot in a private chat (MS Teams).
    /// </summary>
    public static MSTeamsTurnContext CreateUserToBotPrivateMessage(string message)
    {
        var activity = new Activity(ActivityTypes.Message)
        {
            Text = message,
            Id = "1541717374898",
            Timestamp = new DateTime(2018, 1, 30, 23, 59, 59).ToUniversalTime(),
            LocalTimestamp = new DateTime(2018, 1, 30, 23, 59, 59).ToLocalTime(),
            ServiceUrl = "https://service.net/emea/",
            ChannelId = "msteams",
            Conversation = new ConversationAccount(true, "personal", "channel_id"),
            From = new ChannelAccount("Sender_id", "Sender"),
            Recipient = new ChannelAccount("Review_id", "Review"),
            Entities = new List<Entity>(),
            TextFormat = "plain"
        };

        dynamic chanelData = new ExpandoObject();
        chanelData.tenant = new ExpandoObject();
        chanelData.channel = new ExpandoObject();
        chanelData.team = new ExpandoObject();
        chanelData.tenant.id = "tenant_id";
        chanelData.channel.id = "channel_id";
        chanelData.team.id = "team_id";
        activity.ChannelData = chanelData;

        var turnContext = new MSTeamsTurnContext(activity);

        ConvertMentions(activity);

        return turnContext;
    }

    private static void ConvertMentions(Activity activity)
    {
        const string mentionRegex = @"@'([\w\s]+)'";

        foreach (Match match in Regex.Matches(activity.Text, mentionRegex))
        {
            var mentionedUserName = match.Groups[1].Value;
            activity.Entities.Add(
                new Entity("mention")
                {
                    Properties = JObject.FromObject(new Mention(new ChannelAccount(mentionedUserName + "_id", mentionedUserName), $"@{mentionedUserName}", "mention"))
                });
        }

        activity.Text = Regex.Replace(activity.Text, mentionRegex, "@$1");
    }
}