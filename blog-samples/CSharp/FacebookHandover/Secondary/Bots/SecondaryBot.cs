// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FacebookModel;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Secondary.Bots
{
    public class SecondaryBot : ActivityHandler
    {
        /// <summary>
        /// This option passes thread control from the secondary receiver to a primary receiver.
        /// </summary>
        private const string OPTION_PASS_PRIMARY_BOT = "Pass to primary";
        /// <summary>
        /// This option is ignored by this bot.
        /// The primary bot is meant to be listening for this phrase as a standby event
        /// and respond to it by taking thread control.
        /// </summary>
        private const string OPTION_TAKE_THREAD_CONTROL = "Have control taken";
        /// <summary>
        /// This is not an option for this bot,
        /// but this bot is meant to recognize the phrase in a standby event while the primary bot has thread control
        /// and respond to it by requesting thread control from the primary bot.
        /// </summary>
        private const string OPTION_REQUEST_THREAD_CONTROL = "Receive request";
        /// <summary>
        /// This is not an option for this bot,
        /// but this bot is meant to recognize the phrase in a standby event while the primary bot has thread control
        /// and respond to it by requesting thread control from the primary bot with "polite" metadata.
        /// </summary>
        private const string OPTION_REQUEST_THREAD_CONTROL_NICELY = "Receive nice request";

        private static readonly List<Choice> _options = new[] {
            OPTION_PASS_PRIMARY_BOT,
            OPTION_TAKE_THREAD_CONTROL,
        }.Select(option => new Choice(option)).ToList();

        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public SecondaryBot(ILogger<SecondaryBot> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text;

            switch (text)
            {
                case OPTION_PASS_PRIMARY_BOT:
                    await turnContext.SendActivityAsync("Secondary Bot: Passing thread control to the primary receiver...");
                    // A null target app ID will automatically pass control to the primary receiver
                    await FacebookThreadControlHelper.PassThreadControlAsync(_configuration["FacebookPageToken"], null, turnContext.Activity.From.Id, text);
                    break;

                case OPTION_TAKE_THREAD_CONTROL:
                    // Do nothing because the primary receiver should react to this instead
                    break;

                default:
                    await ShowChoices(turnContext, cancellationToken);
                    break;
            }
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SecondaryBot - Processing a ConversationUpdate Activity.");

            var facebookPayload = (turnContext.Activity.ChannelData as JObject)?.ToObject<FacebookPayload>();

            if (facebookPayload != null)
            {
                if (facebookPayload.PassThreadControl != null)
                {
                    await turnContext.SendActivityAsync($"Secondary Bot: Thread control is now passed to {facebookPayload.PassThreadControl.NewOwnerAppId} with the message \"{facebookPayload.PassThreadControl.Metadata}\"");
                    await ShowChoices(turnContext, cancellationToken);
                }
                else if (facebookPayload.TakeThreadControl != null)
                {
                    await turnContext.SendActivityAsync($"Secondary Bot: Thread control was taken by the primary receiver with the message \"{facebookPayload.TakeThreadControl.Metadata}\"."
                        + $" The previous thread owner was {facebookPayload.TakeThreadControl.PreviousOwnerAppId}."
                        + $" Send any message to continue.");
                }
            }

            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }
        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SecondaryBot - Processing an Event Activity.");

            // Analyze Facebook payload from EventActivity.Value
            await ProcessStandbyPayload(turnContext, turnContext.Activity.Value, cancellationToken);
        }

        private async Task ProcessStandbyPayload(ITurnContext turnContext, object data, CancellationToken cancellationToken)
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
                }
            }
        }

        protected virtual async Task OnFacebookStandby(ITurnContext turnContext, FacebookStandby facebookStandby, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SecondaryBot - Standby message received.");

            var text = facebookStandby?.Message?.Text;

            if (text?.Equals(OPTION_REQUEST_THREAD_CONTROL, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                await FacebookThreadControlHelper.RequestThreadControlAsync(_configuration["FacebookPageToken"], facebookStandby.Sender.Id, "give me control");
            }
            else if (text?.Equals(OPTION_REQUEST_THREAD_CONTROL_NICELY, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                await FacebookThreadControlHelper.RequestThreadControlAsync(_configuration["FacebookPageToken"], facebookStandby.Sender.Id, "please");
            }
        }

        private static async Task ShowChoices(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Create the message
            var message = ChoiceFactory.ForChannel(turnContext.Activity.ChannelId, _options, "Secondary Bot: Please type a message or choose an option");
            await turnContext.SendActivityAsync(message, cancellationToken);
        }
    }
}
