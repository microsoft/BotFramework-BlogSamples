using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using ScorableTest.Dialogs;

namespace ScorableTest.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.ConversationUpdate &&
                activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var reply = activity.CreateReply($"[MessagesController] You can interrupt me with IScorable by saying 'check balance' or 'make payment' at any point.  Otherwise, I will just echo back what you say to me!");
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}