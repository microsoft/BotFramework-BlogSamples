namespace basicOperations
{
    public class TopicState
    {
        public enum State { Uninitialized, ChoosingTopic, ChoosingSection, RunningSnippet }

        public State InputState { get; set; } = State.Uninitialized;

        public Bot101.Topic Topic { get; set; } = null;

        public string Section { get; set; } = null;
    }
}