using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReferenceBot
{
    /// <summary>Defines a simple greeting dialog that asks for the user's name.</summary>
    public class MyDialogSet : DialogSet
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
        public MyDialogSet(IStatePropertyAccessor<DialogState> dialogState)
            : base(dialogState)
        {
            this.Add(new TextPrompt(Inputs.Text));
            this.Add(new WaterfallDialog(Name, new WaterfallStep[]
            {
                // Each step takes in a dialog context, arguments, and the next delegate.
                async (dc, step) =>
                {
                    // Prompt for the user's name.
                    return await dc.PromptAsync(Inputs.Text, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?"),
                    });
                },
                async(dc, step) =>
                {
                    var user = (string)step.Result;
                    await dc.Context.SendActivityAsync($"Hi {user}!");
                    return await dc.EndAsync();
                }
            }));
        }
    }
}