// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Luis
{
    /// <summary>
    /// The data structure of the JSON returned by LUIS used for deserialization
    /// </summary>
    [DataContract]
    public class LuisResult
    {
        /// <summary>
        /// Gets or sets the LUIS query that this object was processed from
        /// </summary>
        [DataMember(Name = "query")]
        public string Query { get; set; }

        /// <summary>
        /// Gets the list of intents that the query might relate to
        /// </summary>
        [DataMember(Name = "intents")]
        public ICollection<LuisIntent> Intents { get; private set; }

        /// <summary>
        /// Gets the entities that were identified from the query
        /// </summary>
        [DataMember(Name = "entities")]
        public ICollection<LuisEntity> Entities { get; private set; }
    }
}
