using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ContainerLib
{
    /// <summary>Contains the dialog options for the run a snippet dialog.</summary>
    public class RunSnippetOptions
    {
        public string Section { get; set; }
        public IBot Bot { get; set; }
    }
}
