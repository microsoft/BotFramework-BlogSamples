// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    using System.Collections.Generic;

    /// <summary>Contains information about a user.</summary>
    public class UserProfile
    {
        /// <summary>Gets or sets the user's name.</summary>
        /// <value>The user's name.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the user's age.</summary>
        /// <value>The user's age.</value>
        public int Age { get; set; }

        /// <summary>Gets or sets the list of companies the user wants to review.</summary>
        /// <value>The list of companies the user wants to review.</value>
        public List<string> CompaniesToReview { get; set; } = new List<string>();
    }
}
