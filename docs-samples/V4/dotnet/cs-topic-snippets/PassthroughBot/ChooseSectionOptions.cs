using Microsoft.Bot.Builder.Dialogs;

namespace ContainerLib
{
    /// <summary>Contains the dialog options for the section selection dialog.</summary>
    public class ChooseSectionOptions : DialogOptions
    {
        public TopicDescriptor Topic { get; set; }
    }
}
