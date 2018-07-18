using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;

namespace ManageConversationFlowWithDialogs
{
    public class GreetingDialog : DialogSet
    {
        public const string Main = "greetingDialog";

        private struct Prompts
        {
            public const string Text = "text";
        }

        private struct State
        {
            public const string Name = "name";
            public const string Work = "work";
        }

        public static GreetingDialog Instance = new Lazy<GreetingDialog>(() => new GreetingDialog()).Value;

        private GreetingDialog()
        {
            // Include a text prompt.
            Add(Prompts.Text, new TextPrompt());

            // Define the dialog logic for greeting the user.
            Add(Main, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    dc.ActiveDialog.State = new Dictionary<string,object>();

                    // Ask for their name.
                    await dc.Prompt(Prompts.Text, "What is your name?").ConfigureAwait(false);
                },
                async (dc, args, next) =>
                {
                    // Get the prompt result and save it to state.
                    var name = args["Text"] as string;
                    dc.ActiveDialog.State[State.Name] = name;

                    // Acknowledge their input.
                    await dc.Context.SendActivity($"Pleased to meet you, {name}.").ConfigureAwait(false);

                    // Ask where they work.
                    await dc.Prompt(Prompts.Text, "Where do you work?").ConfigureAwait(false);
                },
                async (dc, args, next) =>
                {
                    // Get the prompt result and save it to state.
                    var work = args["Text"] as string;
                    dc.ActiveDialog.State[State.Work] = work;

                    // Acknowledge their input.
                    await dc.Context.SendActivity($"{work} is a cool place.").ConfigureAwait(false);

                    // End the dialog.
                    await dc.End().ConfigureAwait(false);
                }
            });
        }
    }
}
