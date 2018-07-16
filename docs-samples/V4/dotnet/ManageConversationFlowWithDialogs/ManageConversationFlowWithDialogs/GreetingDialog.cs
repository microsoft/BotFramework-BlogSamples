using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;

namespace ManageConversationFlowWithDialogs
{
    public class GreetingDialog : DialogSet
    {
        public const string Main = "greetingDialog";

        private struct Prompts
        {
            public const string Name = "name";
            public const string Work = "work";
        }

        public static GreetingDialog Instance = new Lazy<GreetingDialog>(() => new GreetingDialog()).Value;

        private GreetingDialog()
        {
            Add(Prompts.Name, new TextPrompt());
            Add(Prompts.Work, new TextPrompt());
            Add(Main, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    dc.ActiveDialog.State = new Dictionary<string,object>();

                    await dc.Prompt(Prompts.Name, "What is your name?");
                },
                async (dc, args, next) =>
                {
                    var name = args["Text"] as string;
                    dc.ActiveDialog.State[Prompts.Name] = name;

                    await dc.Context.SendActivity($"Pleased to meet you, {name}.");

                    await dc.Prompt(Prompts.Work, "Where do you work?");
                },
                async (dc, args, next) =>
                {
                    var work = args["Text"] as string;
                    dc.ActiveDialog.State[Prompts.Work] = work;

                    await dc.Context.SendActivity($"{work} is a cool place.");
                    await dc.End();
                }
            });
        }
    }
}
