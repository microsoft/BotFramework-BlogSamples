// Copyright (c) Microsoft Corporation. All rights reserved.

namespace TriviaBot.Shared
{
    public enum MessageType
    {
        Statement,
        Question,
        GotItRight,
        GotItWrong,
        StartLightningMode,
        StopLightningMode,
        OutOfTime
    }
}