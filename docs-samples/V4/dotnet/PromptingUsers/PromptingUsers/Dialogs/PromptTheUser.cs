using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

namespace PromptTheUser
{
    /// <summary>Defines a simple greeting dialog that asks for the user's name.</summary>
    public class MyDialog : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Name = "mainDialog";

        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the text prompt.</summary>
            public const string Text = "textPrompt";
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        public MyDialog()
        {
            Add(Inputs.Text, new TextPrompt());
            Add(Name, new WaterfallStep[]
            {
                // Each step takes in a dialog context, arguments, and the next delegate.
                async (dc, args, next) =>
                {
                    // Prompt for the user's name.
                    await dc.Prompt(Inputs.Text, "What is your name?").ConfigureAwait(false);
                },
                async(dc, args, next) =>
                {
                    var user = (string)args["Text"];
                    await dc.Context.SendActivity($"Hi {user}!").ConfigureAwait(false);
                    await dc.End().ConfigureAwait(false);
                }
            });
        }
    }
}
