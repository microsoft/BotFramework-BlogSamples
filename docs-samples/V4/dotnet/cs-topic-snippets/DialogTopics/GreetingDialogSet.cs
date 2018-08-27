using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DialogTopics
{
    public class GreetingDialogSet : DialogSet
    {
        public struct Inputs
        {
            public const string Text = "textPrompt";
        }

        public struct Values
        {
            public const string Name = "name";
            public const string WorkPlace = "work";
        }

        public const string Main = "main";

        public GreetingDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            // Include a text prompt.
            Add(new TextPrompt(Inputs.Text));

            // Define the dialog logic for greeting the user.
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                async (dc, step) =>
                {
                    // Ask for their name.
                    return await dc.PromptAsync(Inputs.Text, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?"),
                    });
                },
                async (dc, step) =>
                {
                    // Save the prompt result in dialog state.
                    step.Values[Values.Name] = step.Result as string;

                    // Acknowledge their input.
                    await dc.Context.SendActivityAsync($"Hi, {step.Result}!");

                    // Ask where they work.
                    return await dc.PromptAsync(Inputs.Text, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Where do you work?"),
                    });
                },
                async (dc, step) =>
                {
                    // Save the prompt result in dialog state.
                    step.Values[Values.WorkPlace] = step.Result as string;

                    // Acknowledge their input.
                    await dc.Context.SendActivityAsync($"{step.Result} is a fun place.");

                    // End the dialog.
                    return await dc.EndAsync();
                }
            }));
        }
    }
}
