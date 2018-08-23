using System;
using System.Collections.Generic;
using System.Text;

namespace MetaBot
{
    public class TopicState
    {
        public enum State { Uninitialized, ChoosingTopic, ChoosingSection, RunningSnippet }

        public State InputState { get; set; } = State.Uninitialized;

        public Topic Topic { get; set; } = null;

        public string Section { get; set; } = null;
    }
}
