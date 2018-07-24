using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using System.Collections.Generic;

namespace SpecifyPromptOptions2
{

    /// <summary>Defines a dialog that asks for a choice of color.</summary>
    public class MyDialog : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Name = "mainDialog";

        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the color prompt.</summary>
            public const string Color = "colorPrompt";
        }

        /// <summary>The available colors to choose from.</summary>
        public List<string> Colors = new List<string> { "Green", "Blue" };

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        public MyDialog()
        {
            Add(Inputs.Color, new ChoicePrompt(Culture.English));
            Add(Name, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    // Prompt for a color. A choice prompt requires that you specify choice options.
                    await dc.Prompt(Inputs.Color, "Please make a choice.", new ChoicePromptOptions()
                    {
                        Choices = ChoiceFactory.ToChoices(Colors),
                        RetryPromptActivity =
                            MessageFactory.SuggestedActions(Colors, "Please choose a color.") as Activity
                    }).ConfigureAwait(false);
                },
                async(dc, args, next) =>
                {
                    var color = (FoundChoice)args["Value"];

                    await dc.Context.SendActivity($"You chose {color.Value}.").ConfigureAwait(false);
                    await dc.End().ConfigureAwait(false);
                }
            });
        }
    }
}
