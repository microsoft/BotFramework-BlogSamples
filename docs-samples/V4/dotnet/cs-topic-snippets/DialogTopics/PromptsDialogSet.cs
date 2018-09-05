//#define PromptTheUser
//#define ReusablePromptsA
//#define ReusablePromptsB
//#define ValidateAPromptResponseA
#define ValidateAPromptResponseB
//#define ValidateAPromptResponseC
//#define DefiningChoices

namespace DialogTopics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Recognizers.Text;

    /// <summary>Defines a simple greeting dialog that asks for the user's name.</summary>
    public class PromptsDialogSet : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Main = "mainDialog";

#if PromptTheUser
        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the text prompt.</summary>
            public const string Text = "textPrompt";
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        /// <param name="dialogState">The <see cref="DialogState"/> property accessor for this dialog.</param>
        public PromptsDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            Add(new TextPrompt(Inputs.Text));
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                // Each step takes in a dialog context, step context, and a cancellation token.
                async (dc, step, cancellationToken) =>
                {
                    // Prompt for the user's name.
                    return await dc.PromptAsync(Inputs.Text, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?"),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    var user = step.Result as string;
                    await dc.Context.SendActivityAsync($"Hi {user}!");
                    return await dc.EndAsync();
                },
            }));
        }
#elif ReusablePromptsA
        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the text prompt.</summary>
            public const string Text = "textPrompt";
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        /// <param name="dialogState">The <see cref="DialogState"/> property accessor for this dialog.</param>
        public PromptsDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            Add(new TextPrompt(Inputs.Text));
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                // Each step takes in a dialog context, step context, and a cancellation token.
                async (dc, step, cancellationToken) =>
                {
                    // Prompt for the user's name.
                    return await dc.PromptAsync(Inputs.Text, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?"),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    // Ask them where they work.
                    var user = step.Result as string;
                    return await dc.PromptAsync(Inputs.Text, new PromptOptions
                    {
                        Prompt = MessageFactory.Text($"Hi {user}! Where do you work?"),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    var workPlace = step.Result as string;
                    await dc.Context.SendActivityAsync($"{workPlace} is a cool place!");
                    return await dc.EndAsync();
                },
            }));
        }
#elif ReusablePromptsB
        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the name prompt.</summary>
            public const string Name = "namePrompt";

            /// <summary>The ID of the work prompt.</summary>
            public const string Work = "workPrompt";
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        /// <param name="dialogState">The <see cref="DialogState"/> property accessor for this dialog.</param>
        public PromptsDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            Add(new TextPrompt(Inputs.Name));
            Add(new TextPrompt(Inputs.Work));
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                // Each step takes in a dialog context, step context, and a cancellation token.
                async (dc, step, cancellationToken) =>
                {
                    // Prompt for the user's name.
                    return await dc.PromptAsync(Inputs.Name, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?"),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    // Ask them where they work.
                    var user = step.Result as string;
                    return await dc.PromptAsync(Inputs.Work, new PromptOptions
                    {
                        Prompt = MessageFactory.Text($"Hi {user}! Where do you work?"),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    var workPlace = step.Result as string;
                    await dc.Context.SendActivityAsync($"{workPlace} is a cool place!");
                    return await dc.EndAsync();
                },
            }));
        }
#elif ValidateAPromptResponseA
        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the party size prompt.</summary>
            public const string Size = "parytySize";
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        /// <param name="dialogState">The <see cref="DialogState"/> property accessor for this dialog.</param>
        public PromptsDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            Add(new NumberPrompt<int>(
                dialogId: Inputs.Size,
                validator: async (turnContext, prompt, cancellationToken) =>
                {
                    if (prompt.Recognized.Value >= 6 && prompt.Recognized.Value <= 20)
                    {
                        prompt.End(prompt.Recognized.Value);
                    }
                },
                defaultLocale: Culture.English));
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                // Each step takes in a dialog context, step context, and cancellation token.
                async (dc, step, cancellationToken) =>
                {
                    // Prompt for the party size.
                    return await dc.PromptAsync(Inputs.Size, new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("How many people are in your party?"),
                        RetryPrompt = MessageFactory.Text("Please specify party size between 6 and 20."),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    var size = (int)step.Result;

                    await dc.Context.SendActivityAsync($"Okay, {size} people!");
                    return await dc.EndAsync();
                },
            }));
        }
#elif ValidateAPromptResponseB
        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the party size prompt.</summary>
            public const string Size = "parytySize";
        }

        /// <summary>Validates input for the partySize prompt.</summary>
        /// <param name="turnContext">The context object for the current turn of the bot.</param>
        /// <param name="prompt">The validation context from the prompt.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private static async Task PartySizeValidator(
            ITurnContext turnContext,
            PromptValidatorContext<int> prompt,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (prompt.Recognized.Value >= 6 && prompt.Recognized.Value <= 20)
            {
                prompt.End(prompt.Recognized.Value);
            }
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        /// <param name="dialogState">The <see cref="DialogState"/> property accessor for this dialog.</param>
        public PromptsDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            Add(new NumberPrompt<int>(
                dialogId: Inputs.Size,
                validator: PartySizeValidator,
                defaultLocale: Culture.English));
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                // Each step takes in a dialog context, step context, and cancellation token.
                async (dc, step, cancellationToken) =>
                {
                    // Prompt for the party size.
                    return await dc.PromptAsync(Inputs.Size, new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("How many people are in your party?"),
                        RetryPrompt = MessageFactory.Text("Please specify party size between 6 and 20."),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    var size = (int)step.Result;

                    await dc.Context.SendActivityAsync($"Okay, {size} people!");
                    return await dc.EndAsync();
                },
            }));
        }
#elif ValidateAPromptResponseC
        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the party size prompt.</summary>
            public const string Time = "time";
        }

        /// <summary>Validates input for the Time prompt.</summary>
        /// <param name="turnContext">The context object for the current turn of the bot.</param>
        /// <param name="prompt">The validation context from the prompt.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private static async Task TimeValidator(
            ITurnContext turnContext,
            PromptValidatorContext<IList<DateTimeResolution>> prompt,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (prompt.Recognized.Value.Count == 0)
            {
                await turnContext.SendActivityAsync("Sorry, I did not recognize the time that you entered.");
                return;
            }

            // Find any recognized time that is not in the past.
            var now = DateTime.Now;
            DateTime time = default(DateTime);
            var resolution = prompt.Recognized.Value.FirstOrDefault(
                res => DateTime.TryParse(res.Value, out time) && time > now);

            if (resolution != null)
            {
                prompt.End(resolution);
            }
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        /// <param name="dialogState">The <see cref="DialogState"/> property accessor for this dialog.</param>
        public PromptsDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            Add(new DateTimePrompt(
                dialogId: Inputs.Time,
                validator: TimeValidator,
                defaultLocale: Culture.English));
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                // Each step takes in a dialog context, step context, and cancellation token.
                async (dc, step, cancellationToken) =>
                {
                    // Prompt for the party size.
                    return await dc.PromptAsync(Inputs.Time, new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("When would you like that?"),
                        RetryPrompt = MessageFactory.Text("Please specify a time in the future."),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    var time = (DateTimeResolution)step.Result;

                    await dc.Context.SendActivityAsync($"Okay, {time.Value} it is!");
                    return await dc.EndAsync();
                },
            }));
        }
#elif DefiningChoices
        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the color prompt.</summary>
            public const string Color = "colorPrompt";
        }

        /// <summary>The available colors to choose from.</summary>
        public List<string> Colors = new List<string> { "Red", "Green", "Blue" };

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        /// <param name="dialogState">The <see cref="DialogState"/> property accessor for this dialog.</param>
        public PromptsDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            Add(new ChoicePrompt(Inputs.Color, defaultLocale: Culture.English));
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                // Each step takes in a dialog context, step context, and cancellation token.
                async (dc, step, cancellationToken) =>
                {
                    // Prompt for a color. A choice prompt requires that you specify the available choices.
                    return await dc.PromptAsync(Inputs.Color, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please choose a color?"),
                        RetryPrompt = MessageFactory.Text("Sorry, please choose one of these colors."),
                        Choices = ChoiceFactory.ToChoices(Colors),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    var color = (FoundChoice)step.Result;

                    await dc.Context.SendActivityAsync($"Okay, {color.Value} it is!");
                    return await dc.EndAsync();
                },
            }));
        }
#endif
    }
}
