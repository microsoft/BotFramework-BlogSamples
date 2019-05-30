// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace FacebookModel
{
    /// <summary>
    /// Defines a Facebook PSID.
    /// </summary>
    public class FacebookPsid
    {
        /// <summary>
        /// A Facebook page-scoped ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
