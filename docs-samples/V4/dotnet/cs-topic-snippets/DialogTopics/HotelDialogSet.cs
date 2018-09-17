namespace DialogTopics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Schema;
    using Microsoft.Recognizers.Text;

    /// <summary>Contains the set of dialogs and prompts for the hotel bot.</summary>
    public class HotelDialogSet : DialogSet
    {
        /// <summary>The ID of the top-level dialog.</summary>
        public const string MainMenu = "mainMenu";

        /// <summary>Contains the IDs for the other dialogs in the set.</summary>
        private static class Dialogs
        {
            public const string OrderDinner = "orderDinner";
            public const string OrderPrompt = "orderPrompt";
            public const string ReserveTable = "reserveTable";
        }

        /// <summary>Contains the IDs for the prompts used by the dialogs.</summary>
        private static class Inputs
        {
            public const string Choice = "choicePrompt";
            public const string Number = "numberPrompt";
        }

        /// <summary>Contains the keys used to manage dialog state.</summary>
        private static class Outputs
        {
            public const string OrderCart = "orderCart";
            public const string RoomNumber = "roomNumber";
        }

        /// <summary>Describes an option for the top-level dialog.</summary>
        private class WelcomeChoice
        {
            /// <summary>The text to show the guest for this option.</summary>
            public string Description { get; set; }

            /// <summary>The ID of the associated dialog for this option.</summary>
            public string DialogName { get; set; }
        }

        /// <summary>Describes an option for the food-selection dialog.</summary>
        /// <remarks>We have two types of options. One represents meal items that the guest
        /// can add to their order. The other represents a request to process or cancel the
        /// order.</remarks>
        private class MenuChoice
        {
            /// <summary>The request text for cancelling the meal order.</summary>
            public const string Cancel = "Cancel order";

            /// <summary>The request text for processing the meal order.</summary>
            public const string Process = "Process order";

            /// <summary>The name of the meal item or the request.</summary>
            public string Name { get; set; }

            /// <summary>The price of the meal item; or NaN for a request.</summary>
            public double Price { get; set; }

            /// <summary>The text to show the guest for this option.</summary>
            public string Description => (double.IsNaN(Price)) ? Name : $"{Name} - ${Price:0.00}";
        }

        /// <summary>Contains the lists used to present options to the guest.</summary>
        private static class Lists
        {
            /// <summary>The options for the top-level dialog.</summary>
            public static List<WelcomeChoice> WelcomeOptions { get; } = new List<WelcomeChoice>
            {
                new WelcomeChoice { Description = "Order dinner", DialogName = Dialogs.OrderDinner },
                new WelcomeChoice { Description = "Reserve a table", DialogName = Dialogs.ReserveTable },
            };

            private static List<string> WelcomeList { get; } = WelcomeOptions.Select(x => x.Description).ToList();

            /// <summary>The choices to present in the choice prompt for the top-level dialog.</summary>
            public static IList<Choice> WelcomeChoices { get; } = ChoiceFactory.ToChoices(WelcomeList);

            /// <summary>The reprompt action for the top-level dialog.</summary>
            public static Activity WelcomeReprompt
            {
                get
                {
                    var reprompt = MessageFactory.SuggestedActions(WelcomeList, "Please choose an option");
                    reprompt.AttachmentLayout = AttachmentLayoutTypes.List;
                    return reprompt as Activity;
                }
            }

            /// <summary>The options for the food-selection dialog.</summary>
            public static List<MenuChoice> MenuOptions { get; } = new List<MenuChoice>
            {
                new MenuChoice { Name = "Potato Salad", Price = 5.99 },
                new MenuChoice { Name = "Tuna Sandwich", Price = 6.89 },
                new MenuChoice { Name = "Clam Chowder", Price = 4.50 },
                new MenuChoice { Name = MenuChoice.Process, Price = double.NaN },
                new MenuChoice { Name = MenuChoice.Cancel, Price = double.NaN },
            };

            private static List<string> MenuList { get; } = MenuOptions.Select(x => x.Description).ToList();

            /// <summary>The choices to present in the choice prompt for the food-selection dialog.</summary>
            public static IList<Choice> MenuChoices { get; } = ChoiceFactory.ToChoices(MenuList);

            /// <summary>The reprompt action for the food-selection dialog.</summary>
            public static Activity MenuReprompt
            {
                get
                {
                    var reprompt = MessageFactory.SuggestedActions(MenuList, "Please choose an option");
                    reprompt.AttachmentLayout = AttachmentLayoutTypes.List;
                    return reprompt as Activity;
                }
            }
        }

        /// <summary>Contains the guest's dinner order.</summary>
        private class OrderCart
        {
            public List<MenuChoice> Items { get; set; }
            public double Total { get; set; }
        }

        public HotelDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
        {
            // Add the prompts.
            this.Add(new ChoicePrompt(Inputs.Choice, defaultLocale: Culture.English));
            this.Add(new NumberPrompt<int>(Inputs.Number, defaultLocale: Culture.English));

            // Add the main welcome dialog.
            this.Add(new WaterfallDialog(MainMenu, new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    // Greet the guest and ask them to choose an option.
                    await step.Context.SendActivityAsync("Welcome to Contoso Hotel and Resort.");
                    return await step.PromptAsync(Inputs.Choice, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("How may we serve you today?"),
                        RetryPrompt = Lists.WelcomeReprompt,
                        Choices = Lists.WelcomeChoices,
                    });
                },
                async (step, cancellationToken) =>
                {
                    // Begin a child dialog associated with the chosen option.
                    var choice = step.Result as FoundChoice;
                    var dialogId = Lists.WelcomeOptions[choice.Index].DialogName;

                    return await step.BeginDialogAsync(dialogId);
                },
                async (step, cancellationToken) =>
                {
                    // Start this dialog over again.
                    return await step.ReplaceDialogAsync(MainMenu);
                },
            }));

            // Add the order-dinner dialog.
            this.Add(new WaterfallDialog(Dialogs.OrderDinner, new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("Welcome to our Dinner order service.");

                    // Start the food selection dialog.
                    return await step.BeginDialogAsync(Dialogs.OrderPrompt);
                },
                async (step, cancellationToken) =>
                {
                    if (step.Result is OrderCart order && order?.Items != null && order.Items.Count > 0)
                    {
                        // If there are items in the order, record the order and ask for a room number.
                        step.Values[Outputs.OrderCart] = order.Items;
                        return await step.PromptAsync(Inputs.Number, new PromptOptions
                        {
                            Prompt = MessageFactory.Text("What is your room number?"),
                            RetryPrompt = MessageFactory.Text("Please enter your room number."),
                        });
                    }
                    else
                    {
                        // Otherwise, assume the order was cancelled by the guest and exit.
                        return await step.EndDialogAsync();
                    }
                },
                async (step, cancellationToken) =>
                {
                    // Get and save the guest's answer.
                    var roomNumber = step.Result as string;
                    step.Values[Outputs.RoomNumber] = roomNumber;

                    // Process the dinner order using the collected order cart and room number.
                    await step.Context.SendActivityAsync($"Thank you. Your order will be delivered to room {roomNumber} within 45 minutes.");
                    return await step.EndDialogAsync();
                },
            }));

            // Add the food-selection dialog.
            this.Add(new WaterfallDialog(Dialogs.OrderPrompt, new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    if (step.Options is OrderCart order && order?.Items != null)
                    {
                        // For a continuing order, set the order state to that of the arguments.
                        step.Values[Outputs.OrderCart] = new OrderCart
                        {
                            Items = order.Items.ToList(),
                            Total = order.Total,
                        };
                    }
                    else
                    {
                        // First time through, initialize the order state.
                        step.Values[Outputs.OrderCart] = new OrderCart
                        {
                            Items = new List<MenuChoice>(),
                            Total = 0.0,
                        };
                    }

                    return await step.PromptAsync(Inputs.Choice, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What would you like?"),
                        RetryPrompt = Lists.MenuReprompt,
                        Choices = Lists.MenuChoices,
                    });
                },
                async (step, cancellationToken) =>
                {
                    // Get the guest's choice.
                    var choice = step.Result as FoundChoice;
                    var option = Lists.MenuOptions[choice.Index];

                    // Get the current order from dialog state.
                    var orderCart = (OrderCart)step.Values[Outputs.OrderCart];

                    if (option.Name is MenuChoice.Process)
                    {
                        if (orderCart.Items.Count > 0)
                        {
                            // If there are any items in the order, then exit this dialog,
                            // and return the list of selected food items.
                            return await step.EndDialogAsync(new OrderCart
                            {
                                Items = orderCart.Items.ToList(),
                                Total = orderCart.Total,
                            });
                        }
                        else
                        {
                            // Otherwise, send an error message and restart from
                            // the beginning of this dialog.
                            await step.Context.SendActivityAsync(
                                "Your cart is empty. Please add at least one item to the cart.");
                            return await step.ReplaceDialogAsync(Dialogs.OrderPrompt);
                        }
                    }
                    else if (option.Name is MenuChoice.Cancel)
                    {
                        await step.Context.SendActivityAsync("Your order has been cancelled.");

                        // Exit this dialog, without returning a value.
                        return await step.EndDialogAsync();
                    }
                    else
                    {
                        // Add the selected food item to the order and update the order total.
                        orderCart.Items.Add(option);
                        orderCart.Total += option.Price;

                        await step.Context.SendActivityAsync($"Added {option.Name} (${option.Price:0.00}) to your order." +
                            Environment.NewLine + Environment.NewLine +
                            $"Your current total is ${orderCart.Total:0.00}.");

                        // Present the order options again, passing in the current order state.
                        return await step.ReplaceDialogAsync(Dialogs.OrderPrompt, new OrderCart
                        {
                            Items = orderCart.Items.ToList(),
                            Total = orderCart.Total,
                        });
                    }
                },
            }));

            // Add the table-reservation dialog.
            this.Add(new WaterfallDialog(Dialogs.ReserveTable, new WaterfallStep[]
            {
                // Replace this waterfall with your reservation steps.
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("Your table has been reserved.");
                    return await step.EndDialogAsync();
                }
            }));
        }
    }
}
