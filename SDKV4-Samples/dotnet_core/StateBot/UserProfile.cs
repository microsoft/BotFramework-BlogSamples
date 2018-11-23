// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    public class UserProfile
    {
        public string Name { get; set; }

        // Track whether we have already asked the user's name
        public bool PromptedUserForName { get; set; } = false;
    }
}
