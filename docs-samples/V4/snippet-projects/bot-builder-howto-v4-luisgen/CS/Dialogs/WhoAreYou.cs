using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoCafeBot.Dialogs
{
    public class WhoAreYou : DialogContainer
    {
        public WhoAreYou()
            : base("WhoAreYou")
        {
            Dialogs.Add("WhoAreYou",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State = new Dictionary<string, object>();
                        await dc.Prompt("textPrompt", "Hi, I'm the contoso cafe bot! What's your name?");
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["name"] = args["Value"];
                        await dc.Context.SendActivity($"Hello {args["Value"]}! Nice to meet you.");
                        // TODO: Set this in user state
                        await dc.End(dc.ActiveDialog.State);
                    }
                }
            );
            
            Dialogs.Add("textPrompt", new TextPrompt());
        }
    }
}
