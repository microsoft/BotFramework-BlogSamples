// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace FacebookModel
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
        public FacebookPsid Sender { get; set; }

        /// <summary>
        /// Gets or sets the recipient of the message.
        /// </summary>
        [JsonProperty("recipient")]
        public FacebookPsid Recipient { get; set; }

        /// <summary>
        /// Gets or sets the request_thread_control of the control request.
        /// </summary>
        [JsonProperty("request_thread_control")]
        public FacebookRequestThreadControl RequestThreadControl;

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
