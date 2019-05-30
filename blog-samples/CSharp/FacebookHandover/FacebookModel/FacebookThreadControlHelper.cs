using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FacebookModel
{
    public static class FacebookThreadControlHelper
    {
        public const string GRAPH_API_BASE_URL = "https://graph.facebook.com/v3.3/me/{0}?access_token={1}";

        private static readonly HttpClient _httpClient = new HttpClient();

        private static async Task<bool> PostToFacebookAPIAsync(string postType, string pageToken, string content)
        {
            var requestPath = string.Format(GRAPH_API_BASE_URL, postType, pageToken);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            // Create HTTP transport objects
            using (var requestMessage = new HttpRequestMessage())
            {
                requestMessage.Method = new HttpMethod("POST");
                requestMessage.RequestUri = new Uri(requestPath);
                requestMessage.Content = stringContent;
                requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

                // Make the Http call
                using (var response = await _httpClient.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(false))
                {
                    // Return true if the call was successfull
                    Debug.Print(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    return response.IsSuccessStatusCode;
                }
            }
        }

        public static async Task<List<string>> GetSecondaryReceiversAsync(string pageToken)
        {
            var requestPath = string.Format(GRAPH_API_BASE_URL, "secondary_receivers", pageToken);

            // Create HTTP transport objects
            using (var requestMessage = new HttpRequestMessage())
            {
                requestMessage.Method = new HttpMethod("GET");
                requestMessage.RequestUri = new Uri(requestPath);

                // Make the Http call
                using (var response = await _httpClient.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(false))
                {
                    // Interpret response
                    var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var responseObject = JObject.Parse(responseString);
                    var responseData = responseObject["data"] as JArray;

                    return responseData.Select(receiver => receiver["id"].ToString()).ToList();
                }
            }
        }

        public static async Task<bool> RequestThreadControlAsync(string pageToken, string userId, string message)
        {
            var content = new { recipient = new { id = userId }, metadata = message };
            return await PostToFacebookAPIAsync("request_thread_control", pageToken, JsonConvert.SerializeObject(content)).ConfigureAwait(false);
        }
        
        public static async Task<bool> TakeThreadControlAsync(string pageToken, string userId, string message)
        {
            var content = new { recipient = new { id = userId }, metadata = message };
            return await PostToFacebookAPIAsync("take_thread_control", pageToken, JsonConvert.SerializeObject(content)).ConfigureAwait(false);
        }

        public static async Task<bool> PassThreadControlAsync(string pageToken, string targetAppId, string userId, string message)
        {
            var content = new { recipient = new { id = userId }, target_app_id = targetAppId, metadata = message };
            return await PostToFacebookAPIAsync("pass_thread_control", pageToken, JsonConvert.SerializeObject(content)).ConfigureAwait(false);
        }

        /// <summary>
        /// This extension method populates a turn context's activity with conversation and user information from a Facebook payload.
        /// This is necessary because a turn context needs that information to send messages to a conversation,
        /// and event activities don't necessarily come with that information already in place.
        /// </summary>
        public static void ApplyFacebookPayload(this ITurnContext turnContext, FacebookPayload facebookPayload)
        {
            var userId = facebookPayload.Sender.Id;
            var pageId = facebookPayload.Recipient.Id;
            var conversationId = string.Format("{0}-{1}", userId, pageId);

            turnContext.Activity.From = new ChannelAccount(userId);
            turnContext.Activity.Recipient = new ChannelAccount(pageId);
            turnContext.Activity.Conversation = new ConversationAccount(id: conversationId);
        }
    }
}
