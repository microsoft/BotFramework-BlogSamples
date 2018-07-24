using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

namespace ReusablePrompts2
{
    /// <summary>Defines a simple greeting dialog that asks for the user's name and place of work.</summary>
    public class MyDialog : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Name = "mainDialog";

        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the name prompt.</summary>
            public const string Name = "namePrompt";

            /// <summary>The ID of the work prompt.</summary>
            public const string Work = "workPrompt";
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        public MyDialog()
        {
            Add(Inputs.Name, new TextPrompt());
            Add(Inputs.Work, new TextPrompt());
            Add(Name, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    // Prompt for the user's name.
                    await dc.Prompt(Inputs.Name, "What is your name?").ConfigureAwait(false);
                },
                async(dc, args, next) =>
                {
                    var user = (string)args["Text"];

                    // Ask them where they work.
                    await dc.Prompt(Inputs.Work, $"Hi {user}! Where do you work?").ConfigureAwait(false);
                },
                async(dc, args, next) =>
                {
                    var workplace = (string)args["Text"];

                    await dc.Context.SendActivity($"{workplace} is a cool place!").ConfigureAwait(false);
                    await dc.End().ConfigureAwait(false);
                }
            });
        }
    }
}
