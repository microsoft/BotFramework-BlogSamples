// Copyright (c) Microsoft Corporation. All rights reserved.

namespace TrivaApp
{
    public static class BotConnection
    {
        #error You must specify the direct line secret of the bot to talk to
        #error This requires first publishing the TriviaBot project and configuring it on https://dev.botframework.com/
        public static string DirectLineSecret { get; } = "Your DirectLine secret here";

        #error Please provide a Bing Speech API key, if the Cognitive Services speech recognizer or synthesizer are used."
        public static string BingSpeechKey { get; } = "Your Bing Speech API key here";

        public static string ApplicationName { get; }  = "TriviaApp";
    }
}
