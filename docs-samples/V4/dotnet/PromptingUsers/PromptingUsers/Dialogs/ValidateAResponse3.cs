﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DateTimeResult = Microsoft.Bot.Builder.Prompts.DateTimeResult;
using PromptStatus = Microsoft.Bot.Builder.Prompts.PromptStatus;

namespace ValidateAPromptResponse3
{

    /// <summary>Defines a dialog that asks for the number of people in a party.</summary>
    public class MyDialog : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Name = "mainDialog";

        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the reservation time prompt.</summary>
            public const string Time = "reservationTime";
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        public MyDialog()
        {
            Add(Inputs.Time, new DateTimePrompt(Culture.English, TimeValidator));
            Add(Name, new WaterfallStep[]
            {
                async(dc, args, next) =>
                {
                    // Prompt for a reservation time.
                    await dc.Prompt(Inputs.Time, "When is the reservation for?", new PromptOptions()
                    {
                        RetryPromptString = "Please specify a time."
                    }).ConfigureAwait(false);
                },
                async(dc, args, next) =>
                {
                    var time = ((List<DateTimeResult.DateTimeResolution>)args["Resolution"])[0];
                    await dc.Context.SendActivity($"Your reservation is for {time.Value}.").ConfigureAwait(false);

                    await dc.End().ConfigureAwait(false);
                }
            });
        }

        /// <summary>Validates input for the reservationTime prompt.</summary>
        /// <param name="context">The context object for the current turn of the bot.</param>
        /// <param name="result">The recognition result from the prompt.</param>
        /// <returns>An updated recognition result.</returns>
        private static async Task TimeValidator(ITurnContext context, DateTimeResult result)
        {
            if (result.Resolution.Count == 0)
            {
                await context.SendActivity("Sorry, I did not recognize the time that you entered.").ConfigureAwait(false);
                result.Status = PromptStatus.NotRecognized;
            }

            // Find any recognized time that is not in the past.
            var now = DateTime.Now;
            DateTime time = default(DateTime);
            var resolution = result.Resolution.FirstOrDefault(
                res => DateTime.TryParse(res.Value, out time) && time > now);

            if (resolution != null)
            {
                // If found, keep only that result.
                result.Resolution.Clear();
                result.Resolution.Add(resolution);
            }
            else
            {
                // Otherwise, flag the input as out of range.
                await context.SendActivity("Please enter a time in the future, such as \"tomorrow at 9am\"").ConfigureAwait(false);
                result.Status = PromptStatus.OutOfRange;
            }
        }
    }
}