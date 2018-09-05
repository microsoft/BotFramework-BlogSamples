using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetaBot
{
    public class SelectionDialogSet : DialogSet
    {
        /// <summary>Contains the names of the dialogs and prompts.</summary>
        public struct Inputs
        {
            public const string ChooseTopic = "chooseTopic";
            public const string ChooseSection = "chooseSection";
            public const string RunSnippet = "runSnippet";
            public const string Topic = "topicPrompt";
            public const string Section = "sectionPrompt";
            public const string Run = "passThroughPrompt";
        }

        /// <summary>Contains the keys for step state (the step.Values dictionary).</summary>
        private struct Values
        {
            public const string Topic = "topic";
            public const string Section = "section";
            public const string Bot = "bot";
        }

        /// <summary>Contains "canned" response messages.</summary>
        private struct Responses
        {
            public static IActivity Welcome { get; }
                = MessageFactory.Text($"Welcome to the DialogsBot snippets collection.");
            public static IActivity Error { get; }
                = MessageFactory.Text("I'm sorry, that's not a valid input at this stage.");
            public static IActivity Help { get; }
                = MessageFactory.Text("Type `help` for help, `back` to go back a level, or `reset` to back to topic selection.");
        }

        /// <summary>List of all topics (and snippets) available via this bot.</summary>
        private List<TopicDescriptor> Topics { get; }

        private IActivity ChooseTopic => (_chooseTopic != null) ? _chooseTopic
                    : _chooseTopic = MessageFactory.SuggestedActions(Topics.Select(t => t.Name), "Choose a topic:");
        private IActivity _chooseTopic;

        /// <summary>Returns either the command entered or the index of the topic selected.</summary>
        /// <param name="context">The turn context.</param>
        /// <param name="prompt">The validation context.</param>
        /// <returns>A task representing the operation to perform.</returns>
        private static async Task TopicValidator(
            ITurnContext context,
            PromptValidatorContext<FoundChoice> prompt,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (prompt.Recognized.Succeeded)
            {
                // Return the index of the selected topic.
                prompt.End(prompt.Recognized.Value.Index);
            }
            else
            {
                string text = context.Activity.AsMessageActivity()?.Text?.Trim();
                Command command = Command.Commands.FirstOrDefault(c => c.Equals(text));
                if (command != null)
                {
                    // Return the command entered.
                    prompt.End(command);
                }
            }
        }

        /// <summary>Returns either the command entered or the name of the section selected.</summary>
        /// <param name="context">The turn context.</param>
        /// <param name="prompt">The validation context.</param>
        /// <returns>A task representing the operation to perform.</returns>
        private static async Task SectionValidator(
            ITurnContext context,
            PromptValidatorContext<FoundChoice> prompt,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (prompt.Recognized.Succeeded)
            {
                // Return the name of the selected section.
                prompt.End(prompt.Recognized.Value.Value);
            }
            else
            {
                string text = context.Activity.AsMessageActivity()?.Text?.Trim();
                Command command = Command.Commands.FirstOrDefault(c => c.Equals(text));
                if (command != null)
                {
                    // Return the command entered.
                    prompt.End(command);
                }
            }
        }

        public SelectionDialogSet(IStatePropertyAccessor<DialogState> stateAccessor, List<TopicDescriptor> topics)
            : base(stateAccessor)
        {
            Debug.WriteLine($">>> SelectionDialogState..ctor >> topic count: {topics?.Count()}.");
            Debug.WriteLineIf(topics is null, new StackTrace(true));
            Topics = topics;

            Add(new ChoicePrompt(Inputs.Topic, TopicValidator, defaultLocale: Culture.English));
            Add(new ChoicePrompt(Inputs.Section, SectionValidator, defaultLocale: Culture.English));
            Add(new TextPrompt(Inputs.Run));
            Add(new WaterfallDialog(Inputs.ChooseTopic, new WaterfallStep[]
            {
                async (dc, step, cancellationToken) =>
                {
                    return await dc.PromptAsync(Inputs.Topic, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Choose a topic:"),
                        RetryPrompt = MessageFactory.Text("Please choose one of these topics, or type `help`."),
                        Choices = ChoiceFactory.ToChoices(Topics.Select(t => t.Name).ToList()),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    if (step.Result is Command command)
                    {
                        if (command.Equals(Command.Help))
                        {
                                await dc.Context.SendActivityAsync(Responses.Help);
                        }

                        // All other commands are no-ops, as we're already at the "top level".
                        await dc.Context.TraceActivityAsync("ChooseTopic, step 2: Repeating the choose topic dialog.");
                        return await dc.ReplaceAsync(Inputs.ChooseTopic);
                    }
                    else if (step.Result is int index)
                    {
                        ChooseSectionOptions sectionOptions = new ChooseSectionOptions { Topic = Topics[index] };
                        await dc.Context.TraceActivityAsync($"ChooseTopic, step 2 -- Selected topic **{sectionOptions.Topic.Name}**.");
                        return await dc.BeginAsync(Inputs.ChooseSection, sectionOptions);
                    }

                    // else, we shouldn't get here, but fail gracefully.
                    await dc.Context.TraceActivityAsync("ChooseTopic, step 2, graceful fail: Repeating the choose topic dialog.");
                    return await dc.ReplaceAsync(Inputs.ChooseTopic);
                },
                async (dc, step, cancellationToken) =>
                {
                    Debug.WriteLine("Entering >> Dialog >> ChooseTopic, step 3.");

                    // We're resurfacing from the select-section dialog.
                    // This is the top level, so we don't really care how things bubbled back up.
                    await dc.Context.TraceActivityAsync("ChooseTopic, step 3: Repeating the choose topic dialog.");
                    return await dc.ReplaceAsync(Inputs.ChooseTopic);
                },
            }));
            Add(new WaterfallDialog(Inputs.ChooseSection, new WaterfallStep[]
            {
                async (dc, step, cancellationToken) =>
                {
                    TopicDescriptor topic = (step.Options as ChooseSectionOptions)?.Topic
                        ?? throw new ArgumentNullException("step.Options", "Step options must be provided when begining section selection.");

                    step.Values[Values.Topic] = topic;

                    Debug.WriteLine($"Entering >> Dialog >> ChooseSection, step 1 (for topic {topic.Name}).");

                    return await dc.PromptAsync(Inputs.Section, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Choose a section:"),
                        RetryPrompt = MessageFactory.Text("Please choose one of these sections, or type `help`."),
                        Choices = ChoiceFactory.ToChoices(topic.Sections.Keys.ToList()),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    TopicDescriptor topic = step.Values[Values.Topic] as TopicDescriptor
                        ?? throw new InvalidOperationException("SelectionDialog, step 2 has no Topic value set.");

                    Debug.WriteLine($"Entering >> Dialog >> ChooseSection, step 2 (for topic {topic.Name}).");

                    if (step.Result is Command command)
                    {
                        if (command.Equals(Command.Help))
                        {
                            await dc.Context.SendActivityAsync(Responses.Help);
                            return await dc.ReplaceAsync(Inputs.ChooseSection, new ChooseSectionOptions { Topic = topic });
                        }
                        else if (command.Equals(Command.Back)
                            || command.Equals(Command.Reset))
                        {
                            // Return to the topic selection dialog.
                            await dc.Context.TraceActivityAsync("Exiting the choose section dialog.");
                            return await dc.EndAsync();
                        }
                    }
                    else if (step.Result is string section)
                    {
                        RunSnippetOptions options = new RunSnippetOptions
                        {
                            Section = section,
                            Bot = topic.Sections[section] as IBot,
                        };

                        await dc.Context.TraceActivityAsync($"Starting the run snippet dialog for topic **{topic.Name}**," +
                            $" section **{section}** (`{options.Bot.GetType().Name}`).");
                        return await dc.BeginAsync(Inputs.RunSnippet, options);
                    }

                    // else repeat, using the same initial state, that is, for the same topic.
                    // shouldn't really get here.
                    return await dc.ReplaceAsync(
                        Inputs.ChooseSection,
                        new ChooseSectionOptions { Topic = topic });
                },
                async (dc, step, cancellationToken) =>
                {
                    TopicDescriptor topic = step.Values[Values.Topic] as TopicDescriptor
                        ?? throw new InvalidOperationException("SelectionDialog, step 3 has no Topic value set.");

                    Debug.WriteLine($"Entering >> Dialog >> ChooseSection, step 3 (for topic {topic.Name}).");

                    // We're resurfacing from the run-snippet dialog.
                    // Should only be via a back or reset command.
                    if (step.Result is Command command)
                    {
                        if (command.Equals(Command.Back))
                        {
                            // Repeat, using the same initial state, that is, for the same topic.
                            return await dc.ReplaceAsync(
                                Inputs.ChooseSection,
                                new ChooseSectionOptions { Topic = topic });
                        }
                        else if (command.Equals(Command.Reset))
                        {
                            // Exit and signal that it because of the reset.
                            return await dc.EndAsync(command);
                        }
                        else
                        {
                            // Shouldn't get here, but fail gracefully.
                            await dc.Context.TraceActivityAsync(
                                $"Hit SelectionDialog, step 3 with a {command.Name} command. Repeating the dialog over again.");
                            return await dc.EndAsync();
                        }
                    }
                    else
                    {
                        // Shouldn't get here, but fail gracefully.
                        await dc.Context.TraceActivityAsync(
                            $"Hit SelectionDialog, step 3 with a step.Result of {step.Result ?? "null"}. Repeating the dialog over again.");
                        return await dc.EndAsync();
                    }
                },
            }));
            Add(new WaterfallDialog(Inputs.RunSnippet, new WaterfallStep[]
            {
                async (dc, step, cancellationToken) =>
                {
                    IBot bot = (step.Options as RunSnippetOptions).Bot;
                    step.Values[Values.Bot] = bot;

                    Debug.WriteLine($"Entering >> Dialog >> RunSnippet, step 1 (for bot {bot.GetType().Name}).");

                    string text = dc.Context.Activity.AsMessageActivity().Text?.Trim();
                    if (Command.Help.Equals(text))
                    {
                        await dc.Context.SendActivityAsync(Responses.Help);
                        return await dc.ReplaceAsync(Inputs.RunSnippet, new RunSnippetOptions { Bot = bot });
                    }
                    else if (Command.Back.Equals(text))
                    {
                        return await dc.EndAsync(Command.Back);
                    }
                    else if (Command.Reset.Equals(text))
                    {
                        return await dc.EndAsync(Command.Reset);
                    }
                    else
                    {
                        await bot.OnTurnAsync(dc.Context);
                        return Dialog.EndOfTurn;
                    }
                },
                async (dc, step, cancellationToken) =>
                {
                    IBot bot = step.Values[Values.Bot] as IBot;

                    Debug.WriteLine($"Entering >> Dialog >> RunSnippet, step 2 (for bot {bot.GetType().Name}).");

                    return await dc.ReplaceAsync(Inputs.RunSnippet, new RunSnippetOptions { Bot = bot });
                },
            }));
        }
    }
}
