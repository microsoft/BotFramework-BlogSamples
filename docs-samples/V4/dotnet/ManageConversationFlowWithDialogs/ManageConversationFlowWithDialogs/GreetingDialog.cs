namespace ManageConversationFlowWithDialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using System.Collections.Generic;

    /// <summary>Defines a simple dialog for greeting a user.</summary>
    public class GreetingDialog : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Main = "greetingDialog";

        /// <summary>Defines the IDs of the prompts in the set.</summary>
        private struct Inputs
        {
            public const string Text = "text";
        }

        /// <summary>Defines IDs for output from the dialog.</summary>
        private struct Outputs
        {
            public const string Name = "name";
            public const string Work = "work";
        }

        public GreetingDialog()
        {
            // Include a text prompt.
            Add(Inputs.Text, new TextPrompt());

            // Define the dialog logic for greeting the user.
            Add(Main, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    dc.ActiveDialog.State = new Dictionary<string,object>();

                    // Ask for their name.
                    await dc.Prompt(Inputs.Text, "What is your name?");
                },
                async (dc, args, next) =>
                {
                    // Get the prompt result and save it to state.
                    var name = args["Text"] as string;
                    dc.ActiveDialog.State[Outputs.Name] = name;

                    // Acknowledge their input.
                    await dc.Context.SendActivity($"Hi, {name}!");

                    // Ask where they work.
                    await dc.Prompt(Inputs.Text, "Where do you work?");
                },
                async (dc, args, next) =>
                {
                    // Get the prompt result and save it to state.
                    var work = args["Text"] as string;
                    dc.ActiveDialog.State[Outputs.Work] = work;

                    // Acknowledge their input.
                    await dc.Context.SendActivity($"{work} is a fun place.");

                    // End the dialog.
                    //await dc.End();
                    // Start over from the beginning.
                    await dc.Replace(Main);
                }
            });
        }
    }
}
