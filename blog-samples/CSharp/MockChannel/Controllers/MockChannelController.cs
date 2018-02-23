using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace MockChannel.Controllers
{
    [RoutePrefix("v3/conversations")]
    public class MockChannelController : ApiController
    {
        /// <summary>
        /// CreateConversation
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\CreateConversation.md
        /// </remarks>
        /// <param name="parameters">Parameters to create the conversation from</param>
        [HttpPost]
        [Route("")]
        public HttpResponseMessage CreateConversation([FromBody]ConversationParameters parameters)
        {
            Uri uri = new Uri(Request.RequestUri, "/");
            var id = Guid.NewGuid().ToString("n");
            return Request.CreateResponse(HttpStatusCode.Created, new ConversationResourceResponse(id: id, serviceUrl: uri.ToString()));
        }

        /// <summary>
        /// SendToConversation
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\SendToConversation.md
        /// </remarks>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activity">Activity to send</param>
        [HttpPost]
        [Route("{conversationId}/activities")]
        public HttpResponseMessage SendToConversation(string conversationId, [FromBody]Activity activity)
        {
            var id = Guid.NewGuid().ToString("n");
            return Request.CreateResponse(HttpStatusCode.OK, new ResourceResponse(id: id));
        }

        /// <summary>
        /// ReplyToActivity
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\ReplyToActivity.md
        /// </remarks>
        /// <param name="activity">Activity to send</param>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activityId">activityId the reply is to (OPTIONAL)</param>
        [HttpPost]
        [Route("{conversationId}/activities/{activityId}")]
        public HttpResponseMessage ReplyToActivity(string conversationId, string activityId, [FromBody]Activity activity)
        {
            var id = Guid.NewGuid().ToString("n");
            return Request.CreateResponse(HttpStatusCode.OK, new ResourceResponse(id: id));
        }

        /// <summary>
        /// UpdateActivity
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\UpdateActivity.md
        /// </remarks>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activityId">activityId to update</param>
        /// <param name="activity">replacement Activity</param>
        [HttpPut]
        [Route("{conversationId}/activities/{activityId}")]
        public HttpResponseMessage UpdateActivity(string conversationId, string activityId, [FromBody]Activity activity)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new ResourceResponse(id: activity.Id));
        }

        /// <summary>
        /// DeleteActivity
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\DeleteActivity.md
        /// </remarks>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activityId">activityId to delete</param>
        [HttpDelete]
        [Route("{conversationId}/activities/{activityId}")]
        public HttpResponseMessage DeleteActivity(string conversationId, string activityId)
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// GetConversationMembers
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\GetConversationMembers.md
        /// </remarks>
        /// <param name="conversationId">Conversation ID</param>
        [HttpGet]
        [Route("{conversationId}/members")]
        public HttpResponseMessage GetConversationMembers(string conversationId)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new ChannelAccount[0]);
        }

        /// <summary>
        /// GetActivityMembers
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\GetActivityMembers.md
        /// </remarks>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activityId">Activity ID</param>
        [HttpGet]
        [Route("{conversationId}/activities/{activityId}/members")]
        public HttpResponseMessage GetActivityMembers(string conversationId, string activityId)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new ChannelAccount[0]);
        }

        /// <summary>
        /// UploadAttachment
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\UploadAttachment.md
        /// </remarks>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="attachmentUpload">Attachment data</param>
        [HttpPost]
        [Route("{conversationId}/attachments")]
        public HttpResponseMessage UploadAttachment(string conversationId, [FromBody]AttachmentData attachmentUpload)
        {
            var id = Guid.NewGuid().ToString("n");
            return Request.CreateResponse(HttpStatusCode.OK, new ResourceResponse(id: id));
        }
    }
}
