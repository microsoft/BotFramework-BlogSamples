// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;

namespace TriviaBot.Shared
{
    public class AppEntities
    {
        public string Type = "AppEntities";

        public MessageType? MessageType;
        public IList<string> TriviaAnswerOptions;
    }
}