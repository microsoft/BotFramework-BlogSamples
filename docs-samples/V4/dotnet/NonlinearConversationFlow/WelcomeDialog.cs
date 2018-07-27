using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NonlinearConversationFlow
{
    public class WelcomeDialog : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string MainDialog = "WelcomeDialog";

        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the choice prompt.</summary>
            public const string Choice = "ChoicePrompt";
            public const string Number = "NumberPrompt";
            public const string OrderItem = "OrderItem";
        }

        public struct Outputs
        {
            public const string Order = "Order";
        }

        public static List<DialogDescription> ChildDialogs { get; } = new List<DialogDescription>
            {
                new DialogDescription{ Name = "TableDialog", Description = "Reserve a table", Steps = TableSteps },
                new DialogDescription{ Name = "OrderDialog", Description = "Order dinner", Steps = OrderDinnerSteps },
            };

        private static List<string> MainOptions = ChildDialogs.Select(d => d.Description).ToList();

        public struct DinnerItem
        {
            public string Name { get; set; }
            public double Price { get; set; }
            public override string ToString() => $"{Name} - ${Price:0.00}";
        }

        public class OrderCart : List<DinnerItem>
        {
            public const string Name = "orderCart";
        }

        public static List<DinnerItem> DinnerMenu = new List<DinnerItem>
            {
                new DinnerItem { Name = "Potato Salad", Price = 5.99 },
                new DinnerItem { Name = "Tuna Sandwich", Price = 6.89 },
                new DinnerItem { Name = "Clam Chowder", Price = 4.50 },
            };

        public WelcomeDialog()
        {
            this.Add(Inputs.Choice, new ChoicePrompt(Culture.English));
            this.Add(Inputs.OrderItem, OrderItemSteps);
            foreach (var child in ChildDialogs)
            {
                this.Add(child.Name, child.Steps);
            }
            this.Add(MainDialog, WelcomeSteps);
        }

        private static WaterfallStep[] WelcomeSteps { get; } = new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await dc.Context.SendActivity("Welcome to the Contoso Hotel and Resort.");

                    // Ask the user to choose an option.
                    await dc.Prompt(Inputs.Choice, "How may we serve you today?", new ChoicePromptOptions
                    {
                        Choices = ChoiceFactory.ToChoices(MainOptions),
                        RetryPromptActivity = MessageFactory.SuggestedActions(MainOptions, "Please choose an option.") as Activity,
                    });
                },
                async (dc, args, next) =>
                {
                    // Begin a child dialog associated with the chosen option.
                    var choice = (FoundChoice)args["Value"];
                    var dialog = ChildDialogs.First(d =>
                        d.Description.Equals(choice.Value, StringComparison.InvariantCultureIgnoreCase));
                    await dc.Begin(dialog.Name);
                },
                async (dc, args, next) =>
                {
                    // After the child dialog finishes, clear the dialog stack and start over.
                    await dc.EndAll().Begin(MainDialog);
                },
            };

        private static WaterfallStep[] TableSteps { get; } = new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                },
            };

        private static WaterfallStep[] OrderDinnerSteps { get; } = new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await dc.Context.SendActivity("Welcome to our Dinner order service.");

                    dc.ActiveDialog.State[OrderCart.Name] = new OrderCart();

                    await dc.Begin(Inputs.OrderItem); // Prompt for orders
                },
                async (dc, args, next) =>
                {
                    var result = args[HotelDialogs.Result];
                    if (result.Is(HotelDialogs.Cancel))
                    {
                        await dc.End();
                    }
                    else
                    {
                        // The
                        dc.ActiveDialog.State[Outputs.Order] = result;
                        await dc.Prompt(Inputs.Number, "What is your room number?", new PromptOptions
                        {
                            PromptString = "Please enter your room number."
                        });
                    }
                },
                async (dc, args, next) =>
                {
                },
            };

        private static WaterfallStep[] OrderItemSteps { get; } = new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                },
            };
    }
}
