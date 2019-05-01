using Microsoft.ApplicationInsights;
using Microsoft.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Web.CustomizationBotBuilder
{
    public class CustomActivityLogger : IActivityLogger
    {
        public static string GetValidCloudBlobContainerName(string containerName)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(containerName);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
        public Task LogAsync(IActivity activity)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(activity, Newtonsoft.Json.Formatting.Indented);
            var telemerty = new TelemetryClient(new ApplicationInsights.Extensibility.TelemetryConfiguration(CloudConfigurationManager.GetSetting("APPINSIGHTS_INSTRUMENTATIONKEY")));
            if (activity.AsMessageActivity() != null)
            {
                var message = activity.AsMessageActivity();
                var properties = new Dictionary<string, string>
                    {
                        { "Conversation ID", message.Conversation.Id },
                        { "Channel ID", message.ChannelId },
                        { "Text", message.Text},
                        { "Activity ID", message.Id},
                        { "From ID", message.From.Id},
                        { "From Name",   message.From.Name },
                        { "Recipient ID", message.Recipient.Id},
                        { "Bot ID", CloudConfigurationManager.GetSetting("BotId")},
                        { "Conversation Reference", json},
                        { "Attachments", message.Attachments != null ? Newtonsoft.Json.JsonConvert.SerializeObject(message.Attachments): string.Empty },
                        { "Timestamp", message.Timestamp.HasValue? message.Timestamp.Value.ToString():string.Empty },
                        { "Entities", message.Entities != null? Newtonsoft.Json.JsonConvert.SerializeObject(message.Entities): string.Empty }
                    };
                var messageLocale = message.Locale;
                var country = string.Empty;
                var timezone = string.Empty;
                if (message.Entities != null && message.Entities.Count() > 0)
                {
                    var entity = message.Entities.Where(x => x.Type == "clientInfo").FirstOrDefault();
                    if (entity != null)
                    {
                        messageLocale = entity.Properties["locale"]?.ToString();
                        country = entity.Properties["country"]?.ToString();
                        timezone = entity.Properties["timezone"]?.ToString();
                    }
                }
                if (string.IsNullOrEmpty(messageLocale))
                {
                    var summary = message.From.Properties["summary"];
                    if (summary != null)
                    {
                        messageLocale = summary.Value<string>("locale");
                    }
                }

                properties.Add("Locale", messageLocale);
                properties.Add("Country", country);
                properties.Add("Timezone", timezone);
                properties.Add("Speak", message.Speak);
                properties.Add("ChannelData", message.ChannelData != null ? Newtonsoft.Json.JsonConvert.SerializeObject(message.ChannelData) : string.Empty);              
                properties.Add("ContainerId", GetValidCloudBlobContainerName(message.Conversation.Id));

                bool isBotToUser = GetIsBotToUser(activity);

                if (isBotToUser)
                {
                    properties.Add("User ID", message.Recipient.Id);
                    if (!string.IsNullOrEmpty(message.ReplyToId))
                    {
                        properties.Add("Reply to ID", message.ReplyToId);
                    }
                    telemerty.TrackEvent("Answer", properties);
                }
                else
                {
                    properties.Add("User ID", message.From.Id);
                    telemerty.TrackEvent("Question", properties);
                }
            }

            if (activity.AsTraceActivity() != null)
            {
                var trace = activity.AsTraceActivity();

                var properties = new Dictionary<string, string>
                    {
                        { "Conversation ID", trace.Conversation.Id },
                        { "Channel ID", trace.ChannelId },
                        { "From ID", trace.From.Id},
                        { "From Name", trace.From.Name},
                        { "Recipient ID", trace.Recipient.Id},
                        { "Bot ID", CloudConfigurationManager.GetSetting("BotId")},
                        { "Label", trace.Label},
                        { "Conversation Reference", json},
                        { "Timestamp", trace.Timestamp.HasValue? trace.Timestamp.Value.ToString():string.Empty },
                        { "ReplyToId", trace.ReplyToId },
                        { "Entities", trace.Entities != null? Newtonsoft.Json.JsonConvert.SerializeObject(trace.Entities): string.Empty }
                    };

                if (trace.ValueType == LuisDialog<string>.LuisTraceType)
                {
                    if (trace.Value is LuisTraceInfo traceInfo)
                    {
                        properties.Add("Luis Model ID", traceInfo.LuisModel.ModelID);
                        properties.Add("Top Scoring Intent", traceInfo.LuisResult.TopScoringIntent.Intent);
                        properties.Add("Top Scoring Intent Score", traceInfo.LuisResult.TopScoringIntent.Score?.ToString());
                        properties.Add("Query", traceInfo.LuisResult.Query);
                    }
                }
                properties.Add("ContainerId", GetValidCloudBlobContainerName(trace.Conversation.Id));

                telemerty.TrackEvent("LuisTrace", properties);
            }

            return Task.CompletedTask;
        }

        private static bool GetIsBotToUser(IActivity activity)
        {
            return activity.From?.Id?.ToLower().Contains(CloudConfigurationManager.GetSetting("BotId").ToLower()) == true ||
                    activity.From?.Name?.ToLower().Contains(CloudConfigurationManager.GetSetting("BotId").ToLower()) == true ||
                    activity.From?.Role?.ToLower() == "bot" ||
                    (activity.ChannelId == ChannelIds.Skype && activity.From?.Id?.ToLower() != activity.Conversation.Id?.ToLower()) ||
                    (activity.ChannelId == ChannelIds.Emulator && activity.From?.Id != "default-user");
        }
    }
}
