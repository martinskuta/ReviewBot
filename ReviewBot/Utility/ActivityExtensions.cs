#region using

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Bot.Schema;
using Review.Core.Utility;

#endregion

namespace ReviewBot.Utility
{
    public static class ActivityExtensions
    {
        public static IList<Mention> GetUniqueMentionsExceptRecipient(this IMessageActivity activity)
        {
            return activity.GetMentions()
                           .Where(m => m.Mentioned.Id != activity.Recipient.Id)
                           .DistinctBy(m => m.Mentioned.Id)
                           .ToList();
        }

        public static bool StartsWithRecipientMention(this IMessageActivity activity)
        {
            var recipientMention = activity.GetRecipientMention();
            return recipientMention != null && activity.Text.TrimStart().StartsWith(recipientMention.Text);
        }

        public static bool EndsWithRecipientMention(this IMessageActivity activity)
        {
            var recipientMention = activity.GetRecipientMention();
            return recipientMention != null && activity.Text.TrimEnd().EndsWith(recipientMention.Text);
        }

        public static T AppendMention<T>(this T activity, ChannelAccount mentionedUser, string mentionText = null)
            where T : IMessageActivity
        {
            if (mentionedUser == null || string.IsNullOrEmpty(mentionedUser.Id))
            {
                throw new ArgumentNullException("mentionedUser", "Mentioned user and user ID cannot be null");
            }

            if (string.IsNullOrEmpty(mentionedUser.Name) && string.IsNullOrEmpty(mentionText))
            {
                throw new ArgumentException("Either mentioned user name or mentionText must have a value");
            }

            if (!string.IsNullOrWhiteSpace(mentionText))
            {
                mentionedUser.Name = mentionText;
            }

            var mentionEntityText = string.Format("<at>{0}</at>", mentionedUser.Name);

            activity.Text = activity.Text + mentionEntityText;

            if (activity.Entities == null)
            {
                activity.Entities = new List<Entity>();
            }

            activity.Entities.Add(
                new Mention
                {
                    Text = mentionEntityText,
                    Mentioned = mentionedUser
                });

            return activity;
        }

        public static T AppendNewline<T>(this T activity)
            where T : IMessageActivity
        {
            activity.Text = activity.Text + Environment.NewLine;
            return activity;
        }

        public static T AppendText<T>(this T activity, string text)
            where T : IMessageActivity
        {
            activity.Text = activity.Text + text;
            return activity;
        }

        [CanBeNull]
        private static Mention GetRecipientMention(this IMessageActivity activity)
        {
            return activity.GetMentions().FirstOrDefault(m => m.Mentioned.Id == activity.Recipient.Id);
        }
    }
}