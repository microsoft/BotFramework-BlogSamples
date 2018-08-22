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
                var activity = MessageFactory.Attachment(
                    new Attachment
                    {
                        ContentUrl = "https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg",
                        ContentType = "image/jpeg",
                        Name = "Bot Framework",
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
                var activity = MessageFactory.Attachment(new Attachment[]
                {
                    new Attachment
                    {
                        ContentUrl = "https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png",
                        ContentType = "image/png",
                    },
                    new Attachment
                    {
                        ContentUrl = "https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png",
                        ContentType = "image/png",
                    },
                    new Attachment
                    {
                        ContentUrl = "https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg",
                        ContentType = "image/jpeg",
                    },
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
                var activity = MessageFactory.Attachment(
                    new HeroCard(
                        title: "Get started with the Bot Framework",
                        images: new CardImage[]
                        {
                            new CardImage
                            {
                                Url = "https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg",
                            },
                        },
                        buttons: new CardAction[]
                        {
                            new CardAction
                            {
                                Type = ActionTypes.OpenUrl,
                                Title = "Get started",
                                Value = "https://docs.microsoft.com/en-us/azure/bot-service/",
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
                var activity = MessageFactory.Attachment(
                    new HeroCard(
                        title: "Holler Back Buttons",
                        images: new CardImage[] { new CardImage(url: "imageUrl.png") },
                        buttons: new CardAction[]
                        {
                            new CardAction(title: "Shout Out Loud", type: ActionTypes.ImBack, value: "You can ALL hear me!"),
                            new CardAction(title: "Much Quieter", type: ActionTypes.PostBack, value: "Shh! My Bot friend hears me."),
                            new CardAction(
                                title: "Show me how to Holler",
                                type: ActionTypes.OpenUrl,
                                value: $"https://en.wikipedia.org/wiki/{HeroCard.ContentType}"),
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
                var card = new AdaptiveCard();
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

                var activity = MessageFactory.Attachment(new Attachment(AdaptiveCard.ContentType, content: card));

                // Send the activity as a reply to the user.
                await context.SendActivityAsync(activity, token);
            }
        }

        public class ACarousel : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                // Create the activity and attach a set of Hero cards.
                var activity = MessageFactory.Carousel(
                    new Attachment[]
                    {
                        new HeroCard(
                            title: "Traffic Manager",
                            images: new CardImage[]
                            {
                                new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png"),
                            },
                            buttons: new CardAction[]
                            {
                                new CardAction(title: "Traffic Manager", type: ActionTypes.ImBack, value: "traffic manager"),
                            })
                        .ToAttachment(),
                        new HeroCard(
                            title: "Cloud Service",
                            images: new CardImage[]
                            {
                                new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png"),
                            },
                            buttons: new CardAction[]
                            {
                                new CardAction(title: "Cloud Service", type: ActionTypes.ImBack, value: "cloud service")
                            })
                        .ToAttachment(),
                        new HeroCard(
                            title: "Bot Framework",
                            images: new CardImage[]
                            {
                                new CardImage(url: "https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg"),
                            },
                            buttons: new CardAction[]
                            {
                                new CardAction(title: "Bot Framework", type: ActionTypes.ImBack, value: "bot framework")
                            })
                        .ToAttachment()
                    });

                // Send the activity as a reply to the user.
                await context.SendActivityAsync(activity, token);
            }
        }
    }
}
