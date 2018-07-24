using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromptingUsers
{
    public class MyBot : IBot
    {
        private class DialogInfo
        {
            public string Name;
            public string Description;
            public DialogSet Dialog;
            public string Main;
        }

        private static readonly IReadOnlyList<DialogInfo> Dialogs = GetDialogs();

        private static IActivity LandingCard { get; } = GenerateLandingCard(Dialogs);

        private static IReadOnlyList<DialogInfo> GetDialogs() => new List<DialogInfo>
            {
                new DialogInfo
                {
                    Name = "Prompt",
                    Description = "Prompt the user",
                    Dialog = new PromptTheUser.MyDialog(),
                    Main = PromptTheUser.MyDialog.Name
                },
                new DialogInfo
                {
                    Name = "Reusable 1",
                    Description = "Reusable prompts",
                    Dialog = new ReusablePrompts1.MyDialog(),
                    Main = ReusablePrompts1.MyDialog.Name
                },
                new DialogInfo
                {
                    Name = "Reusable 2",
                    Description = "Reusable prompts, variation 1",
                    Dialog = new ReusablePrompts2.MyDialog(),
                    Main = ReusablePrompts2.MyDialog.Name
                },
                new DialogInfo
                {
                    Name = "Options 1",
                    Description = "Specify prompt options",
                    Dialog = new SpecifyPromptOptions1.MyDialog(),
                    Main = SpecifyPromptOptions1.MyDialog.Name
                },
                new DialogInfo
                {
                    Name = "Options 2",
                    Description = "Specify prompt options, variation 1",
                    Dialog = new SpecifyPromptOptions2.MyDialog(),
                    Main = SpecifyPromptOptions2.MyDialog.Name
                },
                new DialogInfo
                {
                    Name = "Validate 1",
                    Description = "Validate a prompt response",
                    Dialog = new ValidateAPromptResponse1.MyDialog(),
                    Main = ValidateAPromptResponse1.MyDialog.Name
                },
                new DialogInfo
                {
                    Name = "Validate 2",
                    Description = "Validate a prompt response, variation 1",
                    Dialog = new ValidateAPromptResponse2.MyDialog(),
                    Main = ValidateAPromptResponse2.MyDialog.Name
                },
                new DialogInfo
                {
                    Name = "Validate 3",
                    Description = "Validate a prompt response, variation 2",
                    Dialog = new ValidateAPromptResponse3.MyDialog(),
                    Main = ValidateAPromptResponse3.MyDialog.Name
                },
            };

        private static IActivity GenerateLandingCard(IEnumerable<DialogInfo> dialogs) =>
            MessageFactory.SuggestedActions(dialogs.Select(dialog =>
                new CardAction
                {
                    Title = dialog.Name,
                    Text = dialog.Description,
                    DisplayText = dialog.Description,
                    Type = ActionTypes.ImBack,
                    Value = dialog.Name
                }).ToList(),
                text: "Choose a dialog to start:");

        public async Task OnTurn(ITurnContext context)
        {
            // This bot is only handling Messages
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context
                var state = context.GetConversationState<MyState>();
                if (state.DialogNName != null)
                {
                    DialogInfo di = GetDialogByName(state.DialogNName);
                    if (di != null)
                    {
                        var dc = di.Dialog.CreateContext(context, state.DialogState);
                        await dc.Continue().ConfigureAwait(false);
                    }
                }

                if (!context.Responded)
                {
                    state.DialogNName = null;
                    state.DialogState.Clear();

                    var di = GetDialogByName(context.Activity.Text);
                    if (di != null)
                    {
                        state.DialogNName = di.Name;
                        var dc = di.Dialog.CreateContext(context, state.DialogState);
                        await dc.Begin(di.Main).ConfigureAwait(false);
                    }
                }

                if (!context.Responded)
                {
                    // Echo back to the user whatever they typed.
                    await context.SendActivity($"You sent '{context.Activity.Text}'").ConfigureAwait(false);
                    await context.SendActivity(LandingCard).ConfigureAwait(false);
                }
            }
        }

        private static DialogInfo GetDialogByName(string name)
        {
            return Dialogs.FirstOrDefault(
                dialog => dialog.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }    
}
