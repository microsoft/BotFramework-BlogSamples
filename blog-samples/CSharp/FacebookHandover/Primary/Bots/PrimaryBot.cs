// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using FacebookModel;
using System;
using Newtonsoft.Json;
using Microsoft.Bot.Connector;

namespace Primary.Bots
{
    /// <summary>
    /// This is based on Eric Dahlvang's FacebookEventsBotExpanded: https://github.com/EricDahlvang/FacebookEventsBotExpanded
    /// </summary>
    public class PrimaryBot : ActivityHandler
    {
        /// <summary>
        /// This option passes thread control from the primary receiver to the page inbox.
        /// </summary>
        private const string OPTION_PASS_PAGE_INBOX = "Pass to page inbox";
        /// <summary>
        /// This option passes thread control from the primary receiver to a secondary receiver.
        /// </summary>
        private const string OPTION_PASS_SECONDARY_BOT = "Pass to secondary";
        /// <summary>
        /// This option is ignored by this bot.
        /// The secondary bot is meant to be listening for this phrase as a standby event
        /// and respond to it by requesting thread control.
        /// </summary>
        private const string OPTION_REQUEST_THREAD_CONTROL = "Receive request";
        /// <summary>
        /// This option is ignored by this bot.
        /// The secondary bot is meant to be listening for this phrase as a standby event
        /// and respond to it by requesting thread control with "polite" metadata.
        /// </summary>
        private const string OPTION_REQUEST_THREAD_CONTROL_NICELY = "Receive nice request";
        /// <summary>
        /// This is not an option for this bot,
        /// but this bot is meant to recognize the phrase in a standby event while the secondary bot has thread control
        /// and respond to it by taking thread control from the secondary bot.
        /// </summary>
        private const string OPTION_TAKE_THREAD_CONTROL = "Have control taken";
        /// <summary>
        /// The constant ID representing the page inbox
        /// </summary>
        private const string PAGE_INBOX_ID = "263902037430900";

        private static readonly List<Choice> _options = new[] {
            OPTION_PASS_PAGE_INBOX,
            OPTION_PASS_SECONDARY_BOT,
            OPTION_REQUEST_THREAD_CONTROL,
            OPTION_REQUEST_THREAD_CONTROL_NICELY,
        }.Select(option => new Choice(option)).ToList();

        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public PrimaryBot(ILogger<PrimaryBot> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text;

            switch (text)
            {
                case OPTION_PASS_PAGE_INBOX:
                    await turnContext.SendActivityAsync("Primary Bot: Passing thread control to the page inbox...");
                    await FacebookThreadControlHelper.PassThreadControlAsync(_configuration["FacebookPageToken"], PAGE_INBOX_ID, turnContext.Activity.From.Id, text);
                    break;

                case OPTION_PASS_SECONDARY_BOT:

                    var secondaryReceivers = await FacebookThreadControlHelper.GetSecondaryReceiversAsync(_configuration["FacebookPageToken"]);

                    foreach (var receiver in secondaryReceivers)
                    {
                        if (receiver != PAGE_INBOX_ID)
                        {
                            await turnContext.SendActivityAsync($"Primary Bot: Passing thread control to {receiver}...");
                            await FacebookThreadControlHelper.PassThreadControlAsync(_configuration["FacebookPageToken"], receiver, turnContext.Activity.From.Id, text);
                            break;
                        }
                    }

                    break;

                case OPTION_REQUEST_THREAD_CONTROL:
                case OPTION_REQUEST_THREAD_CONTROL_NICELY:
                    // Do nothing because the secondary receiver should react to these instead
                    break;

                default:
                    await ShowChoices(turnContext, cancellationToken);
                    break;
            }
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PrimaryBot - Processing a ConversationUpdate Activity.");

            var facebookPayload = (turnContext.Activity.ChannelData as JObject)?.ToObject<FacebookPayload>();

            if (facebookPayload != null)
            {
                if (facebookPayload.PassThreadControl != null)
                {
                    await turnContext.SendActivityAsync($"Primary Bot: Thread control is now passed to {facebookPayload.PassThreadControl.NewOwnerAppId} with the message \"{facebookPayload.PassThreadControl.Metadata}\"");
                    await ShowChoices(turnContext, cancellationToken);
                }
            }

            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }
        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PrimaryBot - Processing an Event Activity.");

            // Analyze Facebook payload from EventActivity.Value
            await ProcessFacebookMessage(turnContext, turnContext.Activity.Value, cancellationToken);
        }

        private async Task<bool> ProcessFacebookMessage(ITurnContext turnContext, object data, CancellationToken cancellationToken)
        {
            return await ProcessStandbyPayload(turnContext, data, cancellationToken)
                || await ProcessFacebookPayload(turnContext, data, cancellationToken);
        }

        private async Task<bool> ProcessStandbyPayload(ITurnContext turnContext, object data, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name?.Equals("standby", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                var standbys = (data as JObject)?.ToObject<FacebookStandbys>();
                if (standbys != null)
                {
                    foreach (var standby in standbys.Standbys)
                    {
                        await OnFacebookStandby(turnContext, standby, cancellationToken);
                    }

                    return true;
                }
            }

            return false;
        }

        protected virtual async Task OnFacebookStandby(ITurnContext turnContext, FacebookStandby facebookStandby, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PrimaryBot - Standby message received.");

            var text = facebookStandby?.Message?.Text;

            if (text?.Equals(OPTION_TAKE_THREAD_CONTROL, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                await FacebookThreadControlHelper.TakeThreadControlAsync(_configuration["FacebookPageToken"], facebookStandby.Sender.Id, text);
            }
        }

        private async Task<bool> ProcessFacebookPayload(ITurnContext turnContext, object data, CancellationToken cancellationToken)
        {
            try
            {
                var facebookPayload = (data as JObject)?.ToObject<FacebookPayload>();
                if (facebookPayload != null)
                {
                    // At this point we know we are on Facebook channel, and can consume the Facebook custom payload
                    // present in channelData.

                    turnContext.ApplyFacebookPayload(facebookPayload);

                    // Thread Control Request
                    if (facebookPayload.RequestThreadControl != null)
                    {
                        await OnFacebookThreadControlRequest(turnContext, facebookPayload, cancellationToken);
                        return true;
                    }
                }
            }
            catch (JsonSerializationException)
            {
                if (turnContext.Activity.ChannelId != Channels.Facebook)
                {
                    await turnContext.SendActivityAsync("Primary Bot: This sample is intended to be used with a Facebook bot.");
                }
            }

            return false;
        }

        protected virtual async Task OnFacebookThreadControlRequest(ITurnContext turnContext, FacebookPayload facebookPayload, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PrimaryBot - Thread Control Request message received.");

            string requestedOwnerAppId = facebookPayload.RequestThreadControl.RequestedOwnerAppId;

            if (facebookPayload.RequestThreadControl.Metadata == "please")
            {
                await turnContext.SendActivityAsync($"Primary Bot: {requestedOwnerAppId} requested thread control nicely. Passing thread control...");

                var success = await FacebookThreadControlHelper.PassThreadControlAsync(
                    _configuration["FacebookPageToken"],
                    requestedOwnerAppId,
                    facebookPayload.Sender.Id,
                    "allowing thread control");

                if (!success)
                {
                    // Account for situations when the primary receiver doesn't have thread control
                    await turnContext.SendActivityAsync("Primary Bot: Thread control could not be passed.");
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"Primary Bot: {requestedOwnerAppId} requested thread control but did not ask nicely."
                    + " Thread control will not be passed."
                    + " Send any message to continue.");
            }
        }

        private static async Task ShowChoices(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Create the message
            var message = ChoiceFactory.ForChannel(turnContext.Activity.ChannelId, _options, "Primary Bot: Please type a message or choose an option");
            await turnContext.SendActivityAsync(message, cancellationToken);
        }
    }
}
