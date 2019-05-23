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
using Primary.FacebookModel;

namespace Primary.Bots
{
	public class PrimaryBot : ActivityHandler
	{
		const string OPTION_PASS_SECONDARY_BOT = "Pass thread control to the secondary bot";
		const string OPTION_PASS_PAGE_INBOX = "Pass thread control to the page inbox";
		const string OPTION_REQUEST_THREAD_CONTROL = "Have the secondary bot request thread control";

		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;

		public PrimaryBot(ILogger<PrimaryBot> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
		}

		protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
		{
			await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
		}

		protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Processing a ConversationUpdate Activity.");

			var facebookPayload = (turnContext.Activity.ChannelData as JObject)?.ToObject<FacebookPayload>();

			if (facebookPayload != null)
			{
				if (facebookPayload.PassThreadControl != null)
				{
					await turnContext.SendActivityAsync($"Thread control is now passed to: {facebookPayload.PassThreadControl.RequestOwnerAppId} with message: {facebookPayload.PassThreadControl.Metadata}");
					await ShowChoices(turnContext, cancellationToken);
				}
				else if (facebookPayload.TakeThreadControl != null)
				{
					await turnContext.SendActivityAsync($"Thread control is now passed to Primary. Previous thread owner: {facebookPayload.TakeThreadControl.PreviousOwnerAppId} with message: {facebookPayload.TakeThreadControl.Metadata}");
					await ShowChoices(turnContext, cancellationToken);
				}
			}

			await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
		}

		private static async Task ShowChoices(ITurnContext turnContext, CancellationToken cancellationToken)
		{
			// Create choices
			var choices = new List<string> { OPTION_PASS_PAGE_INBOX, OPTION_PASS_SECONDARY_BOT, OPTION_REQUEST_THREAD_CONTROL }
				.Select(option => new Choice(option)).ToList();

			// Create the message
			var message = ChoiceFactory.ForChannel(turnContext.Activity.ChannelId, choices, "Please type a message or choose an option");
			await turnContext.SendActivityAsync(message, cancellationToken);
		}
	}
}
