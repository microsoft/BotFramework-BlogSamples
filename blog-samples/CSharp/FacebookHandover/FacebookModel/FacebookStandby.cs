// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace FacebookModel
{
    /// <summary>
    /// A Facebook stanby event payload definition.
    /// </summary>
    /// <remarks>See <see cref="https://developers.facebook.com/docs/messenger-platform/reference/webhook-events/standby/"> messaging standby Facebook documentation</see>
    /// for more information on standby.</remarks>
    public class FacebookStandbys
    {
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("time")]
        public long Time;
        [JsonProperty("standBy")]
        public FacebookStandby[] Standbys;
    }

    public class FacebookStandby
    {
        [JsonProperty("sender")]
        public FacebookPsid Sender;
        [JsonProperty("recipient")]
        public FacebookPsid Recipient;
        [JsonProperty("timestamp")]
        public long Timestamp;
        [JsonProperty("message")]
        public FacebookStandByMessage Message;
    }
    
    public class FacebookStandByMessage
    {
        [JsonProperty("mid")]
        public string MId;
        [JsonProperty("seq")]
        public long Seq;
        [JsonProperty("text")]
        public string Text;
    }

}
