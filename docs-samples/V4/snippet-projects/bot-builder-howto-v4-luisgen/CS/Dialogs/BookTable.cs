using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace ContosoCafeBot.Dialogs
{
    public class BookTable : DialogContainer
    {
        public BookTable()
            : base("BookTable")
        {

            var promptOptions = new ChoicePromptOptions
            {
                Choices = new List<Choice>
                {
                    new Choice { Value = "Seattle" },
                    new Choice { Value = "Bellevue" },
                    new Choice { Value = "Renton" },
                }
            };

            //Dialogs.Add("textPrompt", new TextPrompt());

            Dialogs.Add("choicePrompt", new ChoicePrompt(Culture.English) { Style = Microsoft.Bot.Builder.Prompts.ListStyle.Auto });
            Dialogs.Add("numberPrompt", new NumberPrompt<int>(Culture.English));
            Dialogs.Add("timexPrompt", new TimexPrompt(Culture.English, TimexValidator));
            Dialogs.Add("confirmationPrompt", new ConfirmPrompt(Culture.English));

            Dialogs.Add("BookTable",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State = new Dictionary<string, object>();
                        IDictionary<string,object> state = dc.ActiveDialog.State;

                        // add any LUIS entities to active dialog state.
                        if(args.ContainsKey("luisResult")) {
                            cafeLUISModel lResult = (cafeLUISModel)args["luisResult"];
                            updateContextWithLUIS(lResult, ref state);
                        }
                        
                        // prompt if we do not already have cafelocation
                        if(state.ContainsKey("cafeLocation")) {
                            state["bookingLocation"] = state["cafeLocation"];
                            await next();
                        } else {
                            await dc.Prompt("choicePrompt", "Which of our locations would you like?", promptOptions);
                        }
                    },
                    async (dc, args, next) =>
                    {
                        var state = dc.ActiveDialog.State;
                        if(!state.ContainsKey("cafeLocation")) {
                            var choiceResult = (FoundChoice)args["Value"];
                            state["bookingLocation"] = choiceResult.Value;
                        }
                        bool promptForDateTime = true;
                        if(state.ContainsKey("datetime")) {
                            // validate timex
                            var inputdatetime = new string[] {(string)state["datetime"]};
                            var results = evaluateTimeX((string[])inputdatetime);
                            if(results.Count != 0) {
                                var timexResolution = results.First().TimexValue;
                                var timexProperty = new TimexProperty(timexResolution.ToString());
                                var bookingDateTime = $"{timexProperty.ToNaturalLanguage(DateTime.Now)}";
                                state["bookingDateTime"] = bookingDateTime;
                                promptForDateTime = false;
                            }
                        }
                        // prompt if we do not already have date and time
                        if(promptForDateTime) {
                            await dc.Prompt("timexPrompt", "When would you like to arrive? (We open at 4PM.)",
                                            new PromptOptions { RetryPromptString = "We only accept reservations for the next 2 weeks and in the evenings between 4PM - 8PM" });
                        } else {
                            await next();
                        }                       
                        
                    },
                    async (dc, args, next) =>
                    {
                        var state = dc.ActiveDialog.State;
                        if(!state.ContainsKey("datetime")) { 
                            var timexResult = (TimexResult)args;
                            var timexResolution = timexResult.Resolutions.First();
                            var timexProperty = new TimexProperty(timexResolution.ToString());
                            var bookingDateTime = $"{timexProperty.ToNaturalLanguage(DateTime.Now)}";
                            state["bookingDateTime"] = bookingDateTime;
                        }
                        // prompt if we already do not have party size
                        if(state.ContainsKey("partySize")) {
                            state["bookingGuestCount"] = state["partySize"];
                            await next();
                        } else {
                            await dc.Prompt("numberPrompt", "How many in your party?");
                        }
                    },
                    async (dc, args, next) =>
                    {
                        var state = dc.ActiveDialog.State;
                        if(!state.ContainsKey("partySize")) {
                            state["bookingGuestCount"] = args["Value"];
                        }

                        await dc.Prompt("confirmationPrompt", $"Thanks, Should I go ahead and book a table for {state["bookingGuestCount"].ToString()} guests at our {state["bookingLocation"].ToString()} location for {state["bookingDateTime"].ToString()} ?");
                    },
                    async (dc, args, next) =>
                    {
                        var dialogState = dc.ActiveDialog.State;

                        // TODO: Verify user said yes to confirmation prompt

                        // TODO: book the table! 

                        await dc.Context.SendActivity($"Thanks, I have {dialogState["bookingGuestCount"].ToString()} guests booked for our {dialogState["bookingLocation"].ToString()} location for {dialogState["bookingDateTime"].ToString()}.");
                    }
                }
            );
        }
        // method to update state
        private void updateContextWithLUIS(cafeLUISModel lResult, ref IDictionary<string,object> dialogContext) {
            if(lResult.Entities.cafeLocation != null && lResult.Entities.cafeLocation.GetLength(0) > 0) {
                dialogContext.Add("cafeLocation", lResult.Entities.cafeLocation[0][0]);
            }
            if(lResult.Entities.partySize != null && lResult.Entities.partySize.GetLength(0) > 0) {
                dialogContext.Add("partySize", lResult.Entities.partySize[0][0]);
            } else {
                if(lResult.Entities.number != null && lResult.Entities.number.GetLength(0) > 0) {
                    dialogContext.Add("partySize", lResult.Entities.number[0]);
                }
            }
            if(lResult.Entities.datetime != null && lResult.Entities.datetime.GetLength(0) > 0) {
                dialogContext.Add("datetime", lResult.Entities.datetime[0].Expressions[0]);
            }
        }
        private List<TimexProperty> evaluateTimeX(string[] candidates) {
            var constraints = new[] {
                TimexCreator.ThisWeek(),                /* Take any entries for this week, no entries from past please */
                TimexCreator.NextWeek(),                /* Take any entries for next week, no dates from the past please */
                TimexCreator.Evening,                   /* Evenings only */
            };

            return TimexRangeResolver.Evaluate(candidates, constraints);
        }
        // The notion of a Validator is a standard pattern across all the Prompts
        private Task TimexValidator(ITurnContext context, TimexResult value)
        {
            var resolutions = evaluateTimeX(value.Resolutions);

            if (resolutions.Count == 0)
            {
                value.Resolutions = new string[] { };
                value.Status = Microsoft.Bot.Builder.Prompts.PromptStatus.OutOfRange;
            }
            else
            {
                value.Resolutions = new[] { resolutions.First().TimexValue };
                value.Status = Microsoft.Bot.Builder.Prompts.PromptStatus.Recognized;
            }

            return Task.CompletedTask;
        }
    }
}
