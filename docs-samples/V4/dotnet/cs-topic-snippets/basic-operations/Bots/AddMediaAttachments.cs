namespace basicOperations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public class AddMediaAttachments
    {
        public class AnAttachment : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                // Create the activity and add an attachment.
                IMessageActivity activity = MessageFactory.Attachment(
                    new Attachment
                    {
                        ContentUrl = "imageUrl.png",
                        ContentType = "image/png",
                        Name = "imageName",
                    }
                );

                // Send the activity to the user.
                await context.SendActivityAsync(activity, token);
            }
        }

        public class Attachments : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                // Create the activity and add an attachment.
                IMessageActivity activity = MessageFactory.Attachment(new Attachment[]
                {
                    new Attachment { ContentUrl = "imageUrl1.png", ContentType = "image/png" },
                    new Attachment { ContentUrl = "imageUrl2.png", ContentType = "image/png" },
                    new Attachment { ContentUrl = "imageUrl3.png", ContentType = "image/png" },
                });

                // Send the activity to the user.
                await context.SendActivityAsync(activity, token);
            }
        }

        public class AHeroCard : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                // Create the activity and attach a Hero card.
                IMessageActivity activity = MessageFactory.Attachment(
                new HeroCard(
                    title: "heroCardTitle",
                    images: new CardImage[] { new CardImage { Url = "imageUrl.png" } },
                    buttons: new CardAction[]
                    {
                        new CardAction
                        {
                            Type = ActionTypes.OpenUrl,
                            Title = "Azure Bot Service Documentation",
                            Value = "https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0",
                        },
                    })
                .ToAttachment());

                // Send the activity as a reply to the user.
                await context.SendActivityAsync(activity, token);
            }
        }

        public class AHeroCardWithEvents : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                // Create the activity and attach a Hero card.
                IMessageActivity activity = MessageFactory.Attachment(
                new HeroCard(
                    title: "Holler Back Buttons",
                    images: new CardImage[] { new CardImage(url: "imageUrl.png") },
                    buttons: new CardAction[]
                    {
                        new CardAction(title: "Shout Out Loud", type: ActionTypes.ImBack, value: "You can ALL hear me!"),
                        new CardAction(title: "Much Quieter", type: ActionTypes.PostBack, value: "Shh! My Bot friend hears me."),
                        new CardAction
                        {
                            Type = ActionTypes.OpenUrl,
                            Title = "Azure Bot Service Documentation",
                            Value = "https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0",
                        },
                    })
                .ToAttachment());

                // Send the activity as a reply to the user.
                await context.SendActivityAsync(activity, token);
            }
        }

        public class AnAdaptiveCard : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                // Create the activity and attach an Adaptive Card.
                AdaptiveCard card = new AdaptiveCard();
                card.Body.Add(new TextBlock()
                {
                    Text = "<title>",
                    Size = TextSize.Large,
                    Wrap = true,
                    Weight = TextWeight.Bolder
                });
                card.Body.Add(new TextBlock() { Text = "<message text>", Wrap = true });
                card.Body.Add(new TextInput()
                {
                    Id = "Title",
                    Value = string.Empty,
                    Style = TextInputStyle.Text,
                    Placeholder = "Title",
                    IsRequired = true,
                    MaxLength = 50
                });
                card.Actions.Add(new SubmitAction() { Title = "Submit", DataJson = "{ Action:'Submit' }" });
                card.Actions.Add(new SubmitAction() { Title = "Cancel", DataJson = "{ Action:'Cancel'}" });

                IMessageActivity activity = MessageFactory.Attachment(new Attachment(AdaptiveCard.ContentType, content: card));

                // Send the activity as a reply to the user.
                await context.SendActivityAsync(activity, token);
            }
        }

        public class ACarousel : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                // Create the activity and attach a set of Hero cards.
                IMessageActivity activity = MessageFactory.Carousel(
                new Attachment[]
                {
                    new HeroCard(
                        title: "cardTitle1",
                        images: new CardImage[] { new CardImage(url: "imageUrl1.png") },
                        buttons: new CardAction[]
                        {
                            new CardAction(title: "buttonTitle1", type: ActionTypes.ImBack, value: "buttonValue1"),
                        })
                    .ToAttachment(),
                    new HeroCard(
                        title: "cardTitle2",
                        images: new CardImage[] { new CardImage(url: "imageUrl2.png") },
                        buttons: new CardAction[]
                        {
                            new CardAction(title: "buttonTitle2", type: ActionTypes.ImBack, value: "buttonValue2")
                        })
                    .ToAttachment(),
                    new HeroCard(
                        title: "cardTitle3",
                        images: new CardImage[] { new CardImage(url: "imageUrl3.png") },
                        buttons: new CardAction[]
                        {
                            new CardAction(title: "buttonTitle3", type: ActionTypes.ImBack, value: "buttonValue3")
                        })
                    .ToAttachment()
                });

                // Send the activity as a reply to the user.
                await context.SendActivityAsync(activity, token);
            }
        }
    }
}
