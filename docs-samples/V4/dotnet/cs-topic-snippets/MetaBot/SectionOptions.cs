using Microsoft.Bot.Builder.Dialogs;

namespace MetaBot
{
    /// <summary>Contains the dialog options for the section selection dialog.</summary>
    public class SectionOptions : DialogOptions
    {
        public Topic Topic { get; set; }
    }
}
