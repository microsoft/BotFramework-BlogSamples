// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;

namespace TriviaBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            try
            {
                await HandleActivity(activity);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return Request.CreateResponse(HttpStatusCode.Accepted);
        }


        private async Task<bool> HandleActivity(Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await HandleMessageActivity(activity);

                return true;
            }
            else if (activity.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (activity.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (activity.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (activity.Type == ActivityTypes.Ping)
            {
            }

            return false;
        }

        private async Task HandleMessageActivity(Activity message)
        {
            /*ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));

            // calculate something for us to return
            int length = (message.Text ?? string.Empty).Length;

            // return our reply to the user
            Activity reply = message.CreateReply($"You sent {message.Text} which was {length} characters");
            await connector.Conversations.ReplyToActivityAsync(reply);

            return false;*/
            await Conversation.SendAsync(message, () => new Runtime.TriviaDialog());
        }
    }
}