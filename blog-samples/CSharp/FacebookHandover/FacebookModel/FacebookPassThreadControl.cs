using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FacebookModel
{
    /// <summary>
    /// A Facebook thread control message, including appid of the new thread owner and an optional message to sent with the request
    /// <see cref="FacebookRequestThreadControl.Metadata"/>
    /// </summary>
    public class FacebookPassThreadControl
    {
        /// <summary>
        /// The app id of the new owner.
        /// </summary>
        /// <remarks>
        /// 263902037430900 for the page inbox.
        /// </remarks>
        [JsonProperty("new_owner_app_id")]
        public string NewOwnerAppId;

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
