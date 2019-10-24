﻿#region copyright

// Copyright 2007 - 2019 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public static bool IsPrivateChat(this IMessageActivity activity) => activity.Conversation.ConversationType == "personal";

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
            activity.Text = activity.Text + "\n\n";
            return activity;
        }

        public static T AppendIndentation<T>(this T activity)
            where T : IMessageActivity
        {
            activity.Text = activity.Text + "  ";
            return activity;
        }

        public static T AppendText<T>(this T activity, string text)
            where T : IMessageActivity
        {
            activity.Text = activity.Text + text;
            return activity;
        }

        [CanBeNull]
        public static Mention GetRecipientMention(this IMessageActivity activity)
        {
            return activity.GetMentions().FirstOrDefault(m => m.Mentioned.Id == activity.Recipient.Id);
        }

        /// <summary>
        ///   Remove recipient mention text from Text property
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>new .Text property value</returns>
        public static string StripRecipientMention(this IMessageActivity activity)
        {
            return activity.StripMentionText(activity.Recipient.Id);
        }

        /// <summary>
        ///   Replace any mention text for given id from Text property
        /// </summary>
        /// <param name="id">id to match</param>
        /// <param name="activity"></param>
        /// <returns>new .Text property value</returns>
        public static string StripMentionText(this IMessageActivity activity, string id)
        {
            var resultText = activity.Text;
            foreach (var mention in activity.GetMentions().Where(mention => mention.Mentioned.Id == id))
            {
                resultText = Regex.Replace(resultText, mention.Text, string.Empty, RegexOptions.IgnoreCase);
            }

            return resultText;
        }

        public static string GetMsTeamsTenantId(this IActivity activity)
        {
            return activity.ChannelData.tenant.id;
        }

        public static string GetMsTeamsChannelId(this IActivity activity)
        {
            return activity.ChannelData.channel.id;
        }

        /// <summary>
        ///   Finds the phrase in the message and splits the text before given phrase into words. Mentions, even if they include
        ///   white space are considered one word.
        /// </summary>
        /// <remarks>
        ///   If the phrase is not found or is empty then empty list is returned.
        /// </remarks>
        public static IList<string> GetWordsBeforePhrase(this IMessageActivity activity, string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return new string[0];

            var message = activity.Text.StripNewLineAndTrim();

            var phraseOccurrenceIdx = message.IndexOf(phrase, StringComparison.OrdinalIgnoreCase);

            if (phraseOccurrenceIdx < 1) return new string[0];

            var textBeforePhrase = message.Substring(0, phraseOccurrenceIdx);

            var splitResult = new List<string>();

            foreach (var mention in activity.GetMentions())
            {
                var replaced = textBeforePhrase.Replace(mention.Text, string.Empty);
                //if we found the mention
                if (replaced.Length != textBeforePhrase.Length)
                {
                    splitResult.Add(mention.Text);
                    textBeforePhrase = replaced;
                }
            }
            //now that we found and removed all mentions from the text, split into words
            splitResult.AddRange(textBeforePhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            return splitResult;
        }
    }
}