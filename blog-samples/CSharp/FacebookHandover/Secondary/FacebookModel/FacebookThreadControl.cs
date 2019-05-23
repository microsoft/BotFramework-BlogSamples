using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Secondary.FacebookModel
{
    /// <summary>
    /// A Facebook thread control message, including appid of requested thread owner and an optional message to send with the request
    /// <see cref="FacebookThreadControl.Metadata"/>
    /// </summary>
    public class FacebookThreadControl
    {
        /// <summary>
        /// The app id of the requested owner.
        /// </summary>
        /// <remarks>
        /// 2149406385095376 for the page inbox.
        /// </remarks>
        [JsonProperty("requested_owner_app_id")]
        public string RequestOwnerAppId; // 2149406385095376 for page

        /// <summary>
        /// Message sent from the requester.
        /// </summary>
        /// <remarks>
        /// Example: "i want the control!"
        /// </remarks>
        [JsonProperty("metadata")]
        public string Metadata;
    }
}
