using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FacebookModel
{
    /// <summary>
    /// A Facebook thread control message, including appid of the previous thread owner and an optional message sent with the request
    /// <see cref="FacebookRequestThreadControl.Metadata"/>
    /// </summary>
    public class FacebookTakeThreadControl
    {
        /// <summary>
        /// The app id of the previous owner.
        /// </summary>
        /// <remarks>
        /// 263902037430900 for the page inbox.
        /// </remarks>
        [JsonProperty("previous_owner_app_id")]
        public string PreviousOwnerAppId;

        /// <summary>
        /// Message sent from the requester.
        /// </summary>
        /// <remarks>
        /// Example: "All yours!"
        /// </remarks>
        [JsonProperty("metadata")]
        public string Metadata;
    }
}
