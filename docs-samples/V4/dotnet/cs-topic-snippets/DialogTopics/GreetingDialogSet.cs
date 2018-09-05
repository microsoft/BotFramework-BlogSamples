namespace DialogTopics
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    public class GreetingDialogSet : DialogSet
    {
        /// <summary>Contains the IDs for the prompts used in the dialog set.</summary>
        private struct Inputs
        {
            public const string Text = "textPrompt";
        }

        /// <summary>Contains the IDs for the values tracked within the dialog set.</summary>
        private struct Values
        {
            public const string Name = "name";
            public const string WorkPlace = "work";
        }

        /// <summary>Defines a class for the information returned by the main dialog.</summary>
        public class Output
        {
            /// <summary>The user's name.</summary>
            public string Name { get; set; }

            /// <summary>The user's place of work.</summary>
            public string WorkPlace { get; set; }
        }

        /// <summary>The name of the main dialog in the set.</summary>
        public const string Main = "main";

        public GreetingDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            // Include a text prompt.
            Add(new TextPrompt(Inputs.Text));

            // Define the dialog logic for greeting the user.
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                async (dc, step, cancellationToken) =>
                {
                    // Ask for their name.
                    return await dc.PromptAsync(Inputs.Text, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?"),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    // Save the prompt result in dialog state.
                    step.Values[Values.Name] = step.Result;

                    // Acknowledge their input.
                    await dc.Context.SendActivityAsync($"Hi, {step.Result}!");

                    // Ask where they work.
                    return await dc.PromptAsync(Inputs.Text, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Where do you work?"),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    // Save the prompt result in dialog state.
                    step.Values[Values.WorkPlace] = step.Result;

                    // Acknowledge their input.
                    await dc.Context.SendActivityAsync($"{step.Result} is a fun place.");

                    // End the dialog and return the collected information.
                    return await dc.EndAsync(new Output
                    {
                        Name = step.Values[Values.Name] as string,
                        WorkPlace = step.Values[Values.WorkPlace] as string,
                    });
                },
            }));
        }
    }
}
