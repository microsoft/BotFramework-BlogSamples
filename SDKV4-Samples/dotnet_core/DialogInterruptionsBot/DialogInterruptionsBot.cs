// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Schema;

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
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class DialogInterruptionsBot : IBot
    {
        // Define the company choices for the company selection prompt.
        private static class Companies
        {
            public const string Adatum = "Adatum Corporation";
            public const string Contoso = "Contoso Suites";
            public const string Gdi = "Graphic Design Institute";
            public const string Wwi = "Wide World Importers";

            public static readonly IReadOnlyDictionary<string, string> Options
                = new Dictionary<string, string>
                    {
                        { Adatum, "A market research company" },
                        { Contoso, "A chain of hotels and inns" },
                        { Gdi, "A school for graphic design" },
                        { Wwi, "An importer of consumer goods" },
                    };

            public static string MoreInfo { get; } =
                string.Join("\r\r", Options.Select(e => $"**{e.Key}**--{e.Value}"));
        }

        // Define interruptions for the conversation.
        private static class Interruptions
        {
            // "Global" interruptions.
            public const string Wait = "wait";
            public const string Continue = "continue";
            public const string Cancel = "cancel";

            // "Local" interruptions (to the review selection process).
            public const string Finish = "finish";
            public const string MoreInfo = "more info";
            public const string Help = "help";

            public static readonly string[] LocalOptions = new string[]
            {
                Finish, MoreInfo, Help,
            };

            public static readonly IReadOnlyDictionary<string, string> Options
                = new Dictionary<string, string>
                    {
                        { Cancel, "Cancel the review sign-up process." },
                        { Continue, "Continues the conversation on hold, if any." },
                        { Finish, "Complete the review sign-up process with the currently selected companies." },
                        { Help, "List the available commands." },
                        { MoreInfo, "Display information about the companies." },
                        { Wait, "Puts the current conversation on hold." },
                    };

            /// <summary>Describes the available commands.</summary>
            public static string HelpText { get; } =
                string.Join("\r\r", Options.Select(e => $"**{e.Key}**--{e.Value}"));
        }

        private const string WelcomeText =
            "Welcome to DialogInterruptionsBot." +
            " This bot provides a complex conversation, supporting various kinds of interruptions." +
            " Type anything to get started.";

        private static string GlobalHelpText { get; } =
            "This bot helps you sign up to review companies." +
            $" To pause the conversation at any time, enter `{Interruptions.Wait}`." +
            $" To resume the conversation, enter `{Interruptions.Continue}`.";

        private const string CancellationText =
            "We have cancelled your sign up. Thankyou.";

        private static string OnHoldText { get; } =
            "The conversation is on hold." +
            $" Enter `{Interruptions.Continue}` to continue the conversation where you left off.";

        // Define the dialog and prompt names for the bot.
        private const string TopLevelDialog = "dialog-topLevel";
        private const string ReviewSelectionDialog = "dialog-reviewSeleciton";
        private const string OnHoldDialog = "dialog-onHold";
        private const string NamePrompt = "prompt-name";
        private const string AgePrompt = "prompt-age";
        private const string SelectionPrompt = "prompt-companySlection";

        // Define value names for values tracked inside the dialogs.
        private const string UserInfo = "value-userInfo";
        private const string CompaniesSelected = "value-companiesSelected";

        /// <summary>
        /// Contains the state property accessors and state management objects for the bot.
        /// </summary>
        private readonly DialogInterruptionsBotAccessors _accessors;

        /// <summary>
        /// The <see cref="DialogSet"/> that contains all the Dialogs that can be used at runtime.
        /// </summary>
        private readonly DialogSet _dialogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogInterruptionsBot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        public DialogInterruptionsBot(DialogInterruptionsBotAccessors accessors)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            // Create a dialog set for the bot. It requires a DialogState accessor, with which
            // to retrieve the dialog state from the turn context.
            _dialogs = new DialogSet(accessors.DialogStateAccessor);

            // Add the prompts we need to the dialog set.
            _dialogs
                .Add(new TextPrompt(NamePrompt))
                .Add(new NumberPrompt<int>(AgePrompt))
                .Add(new ChoicePrompt(SelectionPrompt) { Style = ListStyle.List });

            // Add the dialogs we need to the dialog set.
            _dialogs.Add(new WaterfallDialog(TopLevelDialog)
                .AddStep(NameStepAsync)
                .AddStep(AgeStepAsync)
                .AddStep(StartSelectionStepAsync)
                .AddStep(AcknowledgementStepAsync));

            _dialogs.Add(new WaterfallDialog(ReviewSelectionDialog)
                .AddStep(SelectionStepAsync)
                .AddStep(LoopStepAsync));

            _dialogs.Add(new WaterfallDialog(OnHoldDialog)
                .AddStep(OnHoldStepAsync)
                .AddStep(ContinueToHoldStepAsync));
        }

        /// <summary>
        /// Every conversation turn for our EchoBot will call this method.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                string input = turnContext.Activity.Text?.Trim();

                // Handle any "global" interruptions before continuing.

                // On a request for help, display global help.
                if (string.Equals(input, Interruptions.Help, StringComparison.InvariantCultureIgnoreCase))
                {
                    await turnContext.SendActivityAsync(GlobalHelpText, cancellationToken: cancellationToken);
                    return;
                }

                DialogContext dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);

                // If we're not currently on hold, check whether the user wants to go on hold.
                if (dialogContext.ActiveDialog?.Id != OnHoldDialog)
                {
                    if (string.Equals(input, Interruptions.Wait, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Transition onto hold.
                        await dialogContext.BeginDialogAsync(OnHoldDialog, null, cancellationToken);
                        await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                        return;
                    }
                }

                // On a request to cancel, clear the dialog stack completely.
                if (string.Equals(input, Interruptions.Cancel, StringComparison.InvariantCultureIgnoreCase))
                {
                    await dialogContext.CancelAllDialogsAsync(cancellationToken);
                    await turnContext.SendActivityAsync(CancellationText, cancellationToken: cancellationToken);
                    await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                    return;
                }

                // Run the DialogSet - let the framework identify the current state of the dialog from
                // the dialog stack and figure out what (if any) is the active dialog.
                DialogTurnResult results = await dialogContext.ContinueDialogAsync(cancellationToken);
                switch (results.Status)
                {
                    case DialogTurnStatus.Cancelled:
                    case DialogTurnStatus.Empty:
                        // If there is no active dialog, we should clear the user info and start a new dialog.
                        await _accessors.UserProfileAccessor.SetAsync(turnContext, new UserProfile(), cancellationToken);
                        await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
                        await dialogContext.BeginDialogAsync(TopLevelDialog, null, cancellationToken);
                        break;
                    case DialogTurnStatus.Complete:
                        // If we just finished the dialog, capture and display the results.
                        UserProfile userInfo = results.Result as UserProfile;
                        string status = "You are signed up to review "
                            + (userInfo.CompaniesToReview.Count is 0 ? "no companies" : string.Join(" and ", userInfo.CompaniesToReview))
                            + ".";
                        await turnContext.SendActivityAsync(status);
                        await _accessors.UserProfileAccessor.SetAsync(turnContext, userInfo, cancellationToken);
                        await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
                        break;
                    case DialogTurnStatus.Waiting:
                        // If there is an active dialog, we don't need to do anything here.
                        break;
                }

                await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }

            // Processes ConversationUpdate Activities to welcome the user.
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                // Otherwise, note what type of unexpected activity we just received.
                await turnContext.SendActivityAsync(
                    $"{turnContext.Activity.Type} event detected",
                    cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Sends a welcome message to the user.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        private static async Task SendWelcomeMessageAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            foreach (ChannelAccount member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    Activity reply = turnContext.Activity.CreateReply();
                    reply.Text = WelcomeText;
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }

        /// <summary>The first step of the top-level dialog.</summary>
        /// <param name="stepContext">The waterfall step context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains a <see cref="DialogTurnResult"/> to
        /// communicate some flow control back to the containing WaterfallDialog.</remarks>
        private static async Task<DialogTurnResult> NameStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog.
            stepContext.Values[UserInfo] = new UserProfile();

            Activity prompt = MessageFactory.Text("Please enter your name.");

            // Ask the user to enter their name.
            return await stepContext.PromptAsync(
                NamePrompt,
                new PromptOptions { Prompt = prompt, RetryPrompt = prompt },
                cancellationToken);
        }

        /// <summary>The second step of the top-level dialog.</summary>
        /// <param name="stepContext">The waterfall step context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains a <see cref="DialogTurnResult"/> to
        /// communicate some flow control back to the containing WaterfallDialog.</remarks>
        private static async Task<DialogTurnResult> AgeStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Set the user's name to what they entered in response to the name prompt.
            ((UserProfile)stepContext.Values[UserInfo]).Name = (string)stepContext.Result;

            Activity prompt = MessageFactory.Text("Please enter your age.");

            // Ask the user to enter their age.
            return await stepContext.PromptAsync(
                AgePrompt,
                new PromptOptions { Prompt = prompt, RetryPrompt = prompt },
                cancellationToken);
        }

        /// <summary>The third step of the top-level dialog.</summary>
        /// <param name="stepContext">The waterfall step context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains a <see cref="DialogTurnResult"/> to
        /// communicate some flow control back to the containing WaterfallDialog.</remarks>
        private static async Task<DialogTurnResult> StartSelectionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Set the user's age to what they entered in response to the age prompt.
            int age = (int)stepContext.Result;
            ((UserProfile)stepContext.Values[UserInfo]).Age = age;

            if (age < 25)
            {
                // If they are too young, skip the review selection dialog, and pass an empty list to the next step.
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("You must be 25 or older to participate."),
                    cancellationToken);
                return await stepContext.NextAsync(new List<string>(), cancellationToken);
            }
            else
            {
                // Otherwise, start the review selection dialog.
                return await stepContext.BeginDialogAsync(ReviewSelectionDialog, null, cancellationToken);
            }
        }

        /// <summary>The final step of the top-level dialog.</summary>
        /// <param name="stepContext">The waterfall step context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains a <see cref="DialogTurnResult"/> to
        /// communicate some flow control back to the containing WaterfallDialog.</remarks>
        private static async Task<DialogTurnResult> AcknowledgementStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Set the user's company selection to what they entered in the review-selection dialog.
            List<string> list = stepContext.Result as List<string>;
            var profile = (UserProfile)stepContext.Values[UserInfo];
            profile.CompaniesToReview = list ?? new List<string>();

            // Thank them for participating.
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Thanks for participating, {profile.Name}."),
                cancellationToken);

            // Exit the dialog, returning the collected user information.
            return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
        }

        /// <summary>The first step of the review-selection dialog.</summary>
        /// <param name="stepContext">The waterfall step context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains a <see cref="DialogTurnResult"/> to
        /// communicate some flow control back to the containing WaterfallDialog.</remarks>
        private static async Task<DialogTurnResult> SelectionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Continue using the same selection list, if any, from the previous iteration of this dialog.
            List<string> list = stepContext.Options as List<string> ?? new List<string>();
            stepContext.Values[CompaniesSelected] = list;

            // Create a prompt message.
            string message;
            if (list.Count is 0)
            {
                message = $"Please choose a company to review:";
            }
            else
            {
                message = $"You have selected **{list[0]}**. You can review an additional company:";
            }

            // Create the list of options to choose from.
            List<string> options = Companies.Options.Keys.ToList();
            options.AddRange(Interruptions.LocalOptions);
            if (list.Count > 0)
            {
                options.Remove(list[0]);
            }

            // Prompt the user for a choice.
            return await stepContext.PromptAsync(
                SelectionPrompt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(message),
                    RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                    Choices = ChoiceFactory.ToChoices(options),
                },
                cancellationToken);
        }

        /// <summary>The final step of the review-selection dialog.</summary>
        /// <param name="stepContext">The waterfall step context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains a <see cref="DialogTurnResult"/> to
        /// communicate some flow control back to the containing WaterfallDialog.</remarks>
        private static async Task<DialogTurnResult> LoopStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Retrieve their selection list and the choice they juat made.
            List<string> list = stepContext.Values[CompaniesSelected] as List<string>;
            FoundChoice choice = (FoundChoice)stepContext.Result;

            // Handle any local, expected interruptions appropriately.
            switch (choice.Value)
            {
                case Interruptions.Finish:
                    // Exit and return their current selection list.
                    return await stepContext.EndDialogAsync(list, cancellationToken);
                case Interruptions.Cancel:
                    // Exit and return null.
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                case Interruptions.Help:
                    // Dispaly the help options.
                    await stepContext.Context.SendActivityAsync(
                        Interruptions.HelpText,
                        cancellationToken: cancellationToken);
                    break;
                case Interruptions.MoreInfo:
                    // Display more information about the companies.
                    await stepContext.Context.SendActivityAsync(
                        Companies.MoreInfo,
                        cancellationToken: cancellationToken);
                    break;
                default:
                    // If they chose a company, add it to the list.
                    list.Add(choice.Value);
                    break;
            }

            if (list.Count is 2)
            {
                // If they've selected 2 companies to review, exit and return their list.
                return await stepContext.EndDialogAsync(list, cancellationToken);
            }
            else
            {
                // Otherwise, repeat this dialog, passing in the list from this iteration.
                return await stepContext.ReplaceDialogAsync(ReviewSelectionDialog, list, cancellationToken);
            }
        }

        /// <summary>The first step of the on-hold dialog.</summary>
        /// <param name="stepContext">The waterfall step context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains a <see cref="DialogTurnResult"/> to
        /// communicate some flow control back to the containing WaterfallDialog.</remarks>
        private static async Task<DialogTurnResult> OnHoldStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            string input = stepContext.Context.Activity.Text?.Trim();
            if (string.Equals(input, Interruptions.Continue, StringComparison.InvariantCultureIgnoreCase))
            {
                // Exit and return to the last active dialog state.
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                // Send a status message and let the dialog contnue on the next turn.
                await stepContext.Context.SendActivityAsync(OnHoldText, cancellationToken: cancellationToken);
                return Dialog.EndOfTurn;
            }
        }

        /// <summary>The last step of the on-hold dialog.</summary>
        /// <param name="stepContext">The waterfall step context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains a <see cref="DialogTurnResult"/> to
        /// communicate some flow control back to the containing WaterfallDialog.</remarks>
        private static async Task<DialogTurnResult> ContinueToHoldStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Restart the on-hold dialog.
            return await stepContext.ReplaceDialogAsync(OnHoldDialog, null, cancellationToken);
        }
    }
}
