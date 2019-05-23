// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Secondary.FacebookModel
{
    /// <summary>
    /// Simple version of the payload received from the Facebook channel.
    /// </summary>
    public class FacebookPayload
    {
        /// <summary>
        /// Gets or sets the sender of the message.
        /// </summary>
        [JsonProperty("sender")]
        public FacebookSender Sender { get; set; }

        /// <summary>
        /// Gets or sets the recipient of the message.
        /// </summary>
        [JsonProperty("recipient")]
        public FacebookRecipient Recipient { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [JsonProperty("message")]
        public FacebookMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the postback payload if available.
        /// </summary>
        [JsonProperty("postback")]
        public FacebookPostback PostBack { get; set; }

        /// <summary>
        /// Gets or sets the optin payload if available.
        /// </summary>
        [JsonProperty("optin")]
        public FacebookOptin Optin { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the control request.
        /// </summary>
        [JsonProperty("timestamp")]
        public long Timestamp;

        /// <summary>
        /// Gets or sets the request_thread_control of the control request.
        /// </summary>
        [JsonProperty("request_thread_control")]
        public FacebookThreadControl RequestThreadControl;

        /// <summary>
        /// Gets or sets the pass_thread_control of the control request.
        /// </summary>
        [JsonProperty("pass_thread_control")]
        public FacebookPassThreadControl PassThreadControl;

        /// <summary>
        /// Gets or sets the take_thread_control of the control request.
        /// </summary>
        [JsonProperty("take_thread_control")]
        public FacebookTakeThreadControl TakeThreadControl;
    }
}
