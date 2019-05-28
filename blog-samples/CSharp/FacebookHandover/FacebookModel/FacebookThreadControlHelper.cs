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
		public const string GraphApiBaseUrl = "https://graph.facebook.com/v2.6/me/{0}?access_token={1}";

		private static async Task<bool> PostToFacebookAPIAsync(string postType, string pageToken, string content)
		{
			var requestPath = string.Format(GraphApiBaseUrl, postType, pageToken);
			var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

			// Create HTTP transport objects
			using (var requestMessage = new HttpRequestMessage())
			{
				requestMessage.Method = new HttpMethod("POST");
				requestMessage.RequestUri = new Uri(requestPath);
				requestMessage.Content = stringContent;
				requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

				using (var client = new HttpClient())
				{
					// Make the Http call
					using (var response = await client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(false))
					{
						// Return true if the call was successfull
						Debug.Print(await response.Content.ReadAsStringAsync());
						return response.IsSuccessStatusCode;
					}
				}
			}
		}

		public static async Task<List<string>> GetSecondaryReceiversAsync(string pageToken)
		{
			var requestPath = string.Format(GraphApiBaseUrl, "secondary_receivers", pageToken);

			// Create HTTP transport objects
			using (var requestMessage = new HttpRequestMessage())
			{
				requestMessage.Method = new HttpMethod("GET");
				requestMessage.RequestUri = new Uri(requestPath);

				using (var client = new HttpClient())
				{
					// Make the Http call
					using (var response = await client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(false))
					{
						// Interpret response
						var responseString = await response.Content.ReadAsStringAsync();
						var responseObject = JObject.Parse(responseString);
						var responseData = responseObject["data"] as JArray;

						return responseData.Select(receiver => receiver["id"].ToString()).ToList();
					}
				}
			}
		}

		public static async Task<bool> RequestThreadControlAsync(string pageToken, string userId, string message)
		{
			var content = new { recipient = new { id = userId }, metadata = message };
			return await PostToFacebookAPIAsync("request_thread_control", pageToken, JsonConvert.SerializeObject(content));
		}
		
		public static async Task<bool> TakeThreadControlAsync(string pageToken, string userId, string message)
		{
			var content = new { recipient = new { id = userId }, metadata = message };
			return await PostToFacebookAPIAsync("take_thread_control", pageToken, JsonConvert.SerializeObject(content));
		}

		public static async Task<bool> PassThreadControlAsync(string pageToken, string targetAppId, string userId, string message)
		{
			var content = new { recipient = new { id = userId }, target_app_id = targetAppId, metadata = message };
			return await PostToFacebookAPIAsync("pass_thread_control", pageToken, JsonConvert.SerializeObject(content));
		}

		public static void ApplyFacebookPayloadToTurnContext(ITurnContext turnContext, FacebookPayload facebookPayload)
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
