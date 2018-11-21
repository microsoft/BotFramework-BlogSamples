// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    public class DialogPromptBot : IBot
    {
        // Define identifiers for our dialogs and prompts.
        private const string ReservationDialog = "reservationDialog";
        private const string PartySizePrompt = "partyPrompt";
        private const string LocationPrompt = "locationPrompt";
        private const string ReservationDatePrompt = "reservationDatePrompt";

        private readonly DialogSet _dialogSet;
        private readonly DialogPromptBotAccessors _accessors;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogPromptBot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        public DialogPromptBot(DialogPromptBotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<DialogPromptBot>();
            _logger.LogTrace("EchoBot turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            // Create the dialog set and add the prompts, including custom validation.
            _dialogSet = new DialogSet(_accessors.DialogStateAccessor);
            _dialogSet.Add(new NumberPrompt<int>(PartySizePrompt, PartySizeValidatorAsync));
            _dialogSet.Add(new ChoicePrompt(LocationPrompt));
            _dialogSet.Add(new DateTimePrompt(ReservationDatePrompt, DateValidatorAsync));

            // Define the steps of the waterfall dialog and add it to the set.
            WaterfallStep[] steps = new WaterfallStep[]
            {
                PromptForPartySizeAsync,
                PromptForLocationAsync,
                PromptForReservationDateAsync,
                AcknowledgeReservationAsync,
            };
            _dialogSet.Add(new WaterfallDialog(ReservationDialog, steps));
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                // On a message from the user:
                case ActivityTypes.Message:

                    // Get the current reservation info from state.
                    Reservation reservation = await _accessors.ReservationAccessor.GetAsync(
                        turnContext, () => null, cancellationToken);

                    // Generate a dialog context for our dialog set.
                    DialogContext dc = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);

                    if (dc.ActiveDialog is null)
                    {
                        // If there is no active dialog, check whether we have a reservation yet.
                        if (reservation is null)
                        {
                            // If not, start the dialog.
                            await dc.BeginDialogAsync(ReservationDialog, null, cancellationToken);
                        }
                        else
                        {
                            // Otherwise, send a status message.
                            await turnContext.SendActivityAsync(
                                $"We'll see you on {reservation.Date}.",
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        // Continue the dialog.
                        DialogTurnResult dialogTurnResult = await dc.ContinueDialogAsync(cancellationToken);

                        // If the dialog completed this turn, record the reservation info.
                        if (dialogTurnResult.Status is DialogTurnStatus.Complete)
                        {
                            reservation = (Reservation)dialogTurnResult.Result;
                            await _accessors.ReservationAccessor.SetAsync(
                                turnContext,
                                reservation,
                                cancellationToken);

                            // Send a confirmation message to the user.
                            await turnContext.SendActivityAsync(
                                $"Your party of {reservation.Size} is confirmed for {reservation.Date}.",
                                cancellationToken: cancellationToken);
                        }
                    }

                    // Save the updated dialog state into the conversation state.
                    await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                    break;
            }
        }

        /// <summary>First step of the main dialog: prompt for party size.</summary>
        /// <param name="stepContext">The context for the waterfall step.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains information from this step.</remarks>
        private async Task<DialogTurnResult> PromptForPartySizeAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Prompt for the party size. The result of the prompt is returned to the next step of the waterfall.
            return await stepContext.PromptAsync(
                PartySizePrompt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("How many people is the reservation for?"),
                    RetryPrompt = MessageFactory.Text("How large is your party?"),
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForLocationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Record the party size information in the current dialog state.
            int size = (int)stepContext.Result;
            stepContext.Values["size"] = size;

            return await stepContext.PromptAsync(
                "locationPrompt",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please choose a location."),
                    RetryPrompt = MessageFactory.Text("Sorry, please choose a location from the list."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Redmond", "Bellevue", "Seattle" }),
                },
                cancellationToken);
        }

        /// <summary>Second step of the main dialog: record the party size and prompt for the
        /// reservation date.</summary>
        /// <param name="stepContext">The context for the waterfall step.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains information from this step.</remarks>
        private async Task<DialogTurnResult> PromptForReservationDateAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Record the party size information in the current dialog state.
            var location = stepContext.Result;
            stepContext.Values["location"] = location;

            // Prompt for the party size. The result of the prompt is returned to the next step of the waterfall.
            return await stepContext.PromptAsync(
                ReservationDatePrompt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Great. When will the reservation be for?"),
                    RetryPrompt = MessageFactory.Text("What time should we make your reservation for?"),
                },
                cancellationToken);
        }

        /// <summary>Third step of the main dialog: return the collected party size and reservation date.</summary>
        /// <param name="stepContext">The context for the waterfall step.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains information from this step.</remarks>
        private async Task<DialogTurnResult> AcknowledgeReservationAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Retrieve the reservation date.
            DateTimeResolution resolution = (stepContext.Result as IList<DateTimeResolution>).First();
            string time = resolution.Value ?? resolution.Start;

            // Send an acknowledgement to the user.
            await stepContext.Context.SendActivityAsync(
                "Thank you. We will confirm your reservation shortly.",
                cancellationToken: cancellationToken);

            // Return the collected information to the parent context.
            Reservation reservation = new Reservation
            {
                Date = time,
                Size = (int)stepContext.Values["size"],
            };
            return await stepContext.EndDialogAsync(reservation, cancellationToken);
        }

        /// <summary>Validates whether the party size is appropriate to make a reservation.</summary>
        /// <param name="promptContext">The validation context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Reservations can be made for groups of 6 to 20 people.
        /// If the task is successful, the result indicates whether the input was valid.</remarks>
        private async Task<bool> PartySizeValidatorAsync(
            PromptValidatorContext<int> promptContext,
            CancellationToken cancellationToken)
        {
            // Check whether the input could be recognized as an integer.
            if (!promptContext.Recognized.Succeeded)
            {
                await promptContext.Context.SendActivityAsync(
                    "I'm sorry, I do not understand. Please enter the number of people in your party.",
                    cancellationToken: cancellationToken);
                return false;
            }

            // Check whether the party size is appropriate.
            int size = promptContext.Recognized.Value;
            if (size < 6 || size > 20)
            {
                await promptContext.Context.SendActivityAsync(
                    "Sorry, we can only take reservations for parties of 6 to 20.",
                    cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        /// <summary>Validates whether the reservation date is appropriate.</summary>
        /// <param name="promptContext">The validation context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Reservations must be made at least an hour in advance.
        /// If the task is successful, the result indicates whether the input was valid.</remarks>
        private async Task<bool> DateValidatorAsync(
            PromptValidatorContext<IList<DateTimeResolution>> promptContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check whether the input could be recognized as an integer.
            if (!promptContext.Recognized.Succeeded)
            {
                await promptContext.Context.SendActivityAsync(
                    "I'm sorry, I do not understand. Please enter the date or time for your reservation.",
                    cancellationToken: cancellationToken);
                return false;
            }

            // Check whether any of the recognized date-times are appropriate,
            // and if so, return the first appropriate date-time.
            DateTime earliest = DateTime.Now.AddHours(1.0);
            DateTimeResolution value = promptContext.Recognized.Value.FirstOrDefault(v =>
                DateTime.TryParse(v.Value ?? v.Start, out DateTime time) && DateTime.Compare(earliest, time) <= 0);
            if (value != null)
            {
                promptContext.Recognized.Value.Clear();
                promptContext.Recognized.Value.Add(value);
                return true;
            }

            await promptContext.Context.SendActivityAsync(
                    "I'm sorry, we can't take reservations earlier than an hour from now.",
                    cancellationToken: cancellationToken);
            return false;
        }

        public class Reservation
        {
            public int Size { get; set; }

            public string Date { get; set; }
        }
    }
}
