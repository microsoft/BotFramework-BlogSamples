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
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Secondary.Bots
{
	public class SecondaryBot : ActivityHandler
	{
		private const string OPTION_PASS_PAGE_INBOX = "Pass to page inbox";
		private const string OPTION_PASS_PRIMARY_BOT = "Pass to primary";
		private const string OPTION_REQUEST_THREAD_CONTROL = "Receive request";
		private const string OPTION_REQUEST_THREAD_CONTROL_NICELY = "Receive nice request";
		private const string OPTION_TAKE_THREAD_CONTROL = "Have control taken";
		private const string PAGE_INBOX_ID = "263902037430900";

		private static readonly string[] _options = new[] { OPTION_PASS_PAGE_INBOX, OPTION_PASS_PRIMARY_BOT, OPTION_TAKE_THREAD_CONTROL };

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
				case OPTION_PASS_PAGE_INBOX:
					await turnContext.SendActivityAsync("Passing thread control to the page inbox.");
					await FacebookThreadControlHelper.PassThreadControlAsync(_configuration["FacebookPageToken"], PAGE_INBOX_ID, turnContext.Activity.From.Id, text);
					break;

				case OPTION_PASS_PRIMARY_BOT:
					await turnContext.SendActivityAsync("Passing thread control to the primary app.");
					await FacebookThreadControlHelper.PassThreadControlAsync(_configuration["FacebookPageToken"], _configuration["FacebookPrimaryAppID"], turnContext.Activity.From.Id, text);

					break;

				case OPTION_TAKE_THREAD_CONTROL:
					// Do nothing because the primary receiver should react to this instead
					break;

				default:
					await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {text}"), cancellationToken);
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
					await turnContext.SendActivityAsync($"Thread control is now passed to {facebookPayload.PassThreadControl.NewOwnerAppId} with the message: {facebookPayload.PassThreadControl.Metadata}");
					await ShowChoices(turnContext, cancellationToken);
				}
				else if (facebookPayload.TakeThreadControl != null)
				{
					await turnContext.SendActivityAsync($"Thread control is now passed to the primary app with the message: {facebookPayload.TakeThreadControl.Metadata}. Previous thread owner: {facebookPayload.TakeThreadControl.PreviousOwnerAppId}");
					await ShowChoices(turnContext, cancellationToken);
				}
			}

			await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
		}
		protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
		{
			_logger.LogInformation("SecondaryBot - Processing an Event Activity.");

			// Analyze Facebook payload from EventActivity.Value
			await ProcessFacebookMessage(turnContext, turnContext.Activity.Value, cancellationToken);
		}

		private async Task<bool> ProcessFacebookMessage(ITurnContext turnContext, object data, CancellationToken cancellationToken)
		{
			return await ProcessStandbyPayload(turnContext, data, cancellationToken);
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
						return true;
					}
				}
			}

			return false;
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
			// Create choices
			var choices = _options.Select(option => new Choice(option)).ToList();

			// Create the message
			var message = ChoiceFactory.ForChannel(turnContext.Activity.ChannelId, choices, "Please type a message or choose an option");
			await turnContext.SendActivityAsync(message, cancellationToken);
		}
	}
}
