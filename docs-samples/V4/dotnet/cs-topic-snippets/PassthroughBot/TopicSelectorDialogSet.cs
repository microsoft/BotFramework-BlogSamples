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

namespace ContainerLib
{
    public class TopicSelectorDialogSet : DialogSet, IMetaDialogSet
    {
        public class StatePropertyAccessors
        {
            public IStatePropertyAccessor<DialogState> DialogState { get; set; }
        }

        /// <summary>Contains the names of the dialogs and prompts.</summary>
        public struct Inputs
        {
            /// <summary>The topic-selection dialog.</summary>
            public const string ChooseTopic = "chooseTopic";

            /// <summary>The section-selection dialog.</summary>
            public const string ChooseSection = "chooseSection";

            /// <summary>The run-the-selected-bot dialog.</summary>
            public const string RunSnippet = "runSnippet";

            /// <summary>Prompt for the topic-selection dialog.</summary>
            public const string Topic = "topicPrompt";

            /// <summary>Prompt for the section-selection dialog.</summary>
            public const string Section = "sectionPrompt";

            /// <summary>Prompt for input t be sent to the selected bot.</summary>
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
                = MessageFactory.Text(
                    "Type `help` for help, `back` to go back a level, " +
                    "or `reset` to back to topic selection.");
        }

        /// <InheritDoc/>
        public string Default => Inputs.ChooseTopic;

        /// <InheritDoc/>
        public string Name => "a topic-selection bot";

        private async Task<bool> TopicValidator(
            PromptValidatorContext<FoundChoice> promptContext,
            CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                // Return the index of the selected topic.
                //prompt.End(prompt.Recognized.Value.Index);
                return true;
            }
            else
            {
                string text = promptContext.Context.Activity.AsMessageActivity()?.Text?.Trim();
                Command command = Command.Commands.FirstOrDefault(c => c.Equals(text));
                if (command != null)
                {
                    // Return the command entered.
                    //prompt.End(command);
                    promptContext.State["command"] = command;
                    return true;
                }
                return false;
            }
        }

        /// <summary>Returns either the command entered or the name of the section selected.</summary>
        /// <param name="context">The turn context.</param>
        /// <param name="promptContext">The validation context.</param>
        /// <returns>A task representing the operation to perform.</returns>
        private static async Task<bool> SectionValidator(
            PromptValidatorContext<FoundChoice> promptContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (promptContext.Recognized.Succeeded)
            {
                // Return the name of the selected section.
                return true;
            }
            else
            {
                string text = promptContext.Context.Activity.AsMessageActivity()?.Text?.Trim();
                Command command = Command.Commands.FirstOrDefault(c => c.Equals(text));
                if (command != null)
                {
                    // Return the command entered.
                    promptContext.State["command"] = command;
                    return true;
                }
                return false;
            }
        }

        /// <summary>List of all topics (and snippets) available via this bot.</summary>
        private List<TopicDescriptor> Topics { get; }

        private ConversationState ConvState { get; }

        private IActivity ChooseTopic => (_chooseTopic != null) ? _chooseTopic
                    : _chooseTopic = MessageFactory.SuggestedActions(Topics.Select(t => t.Name), "Choose a topic:");
        private IActivity _chooseTopic;

        public TopicSelectorDialogSet(
            IStatePropertyAccessor<DialogState> dialogState,
            List<TopicDescriptor> topics,
            ConversationState convState)
            : base(dialogState)
        {
            Topics = topics ?? throw new ArgumentNullException(nameof(topics));

            Add(new ChoicePrompt(Inputs.Topic, TopicValidator, defaultLocale: Culture.English));
            Add(new ChoicePrompt(Inputs.Section, SectionValidator, defaultLocale: Culture.English));
            Add(new TextPrompt(Inputs.Run));

            Add(new WaterfallDialog(Inputs.ChooseTopic, new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    return await step.PromptAsync(Inputs.Topic, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Choose a topic:"),
                        RetryPrompt = MessageFactory.Text("Please choose one of these topics, or type `help`."),
                        Choices = ChoiceFactory.ToChoices(Topics.Select(t => t.Name).ToList()),
                    });
                },
                async (step, cancellationToken) =>
                {
                    if (step.Result is Command command)
                    {
                        if (command.Equals(Command.Help))
                        {
                                await step.Context.SendActivityAsync(Responses.Help);
                        }

                        // All other commands are no-ops, as we're already at the "top level".
                        await step.Context.TraceActivityAsync("ChooseTopic, step 2: Repeating the choose topic dialog.");
                        return await step.ReplaceDialogAsync(Inputs.ChooseTopic);
                    }
                    else if (step.Result is int index)
                    {
                        ChooseSectionOptions sectionOptions = new ChooseSectionOptions { Topic = Topics[index] };
                        await step.Context.TraceActivityAsync($"ChooseTopic, step 2 -- Selected topic **{sectionOptions.Topic.Name}**.");
                        return await step.BeginDialogAsync(Inputs.ChooseSection, sectionOptions);
                    }

                    // else, we shouldn't get here, but fail gracefully.
                    await step.Context.TraceActivityAsync("ChooseTopic, step 2, graceful fail: Repeating the choose topic dialog.");
                    return await step.ReplaceDialogAsync(Inputs.ChooseTopic);
                },
                async (step, cancellationToken) =>
                {
                    Debug.WriteLine("Entering >> Dialog >> ChooseTopic, step 3.");

                    // We're resurfacing from the select-section dialog.
                    // This is the top level, so we don't really care how things bubbled back up.
                    await step.Context.TraceActivityAsync("ChooseTopic, step 3: Repeating the choose topic dialog.");
                    return await step.ReplaceDialogAsync(Inputs.ChooseTopic);
                },
            }));

            Add(new WaterfallDialog(Inputs.ChooseSection, new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    TopicDescriptor topic = (step.Options as ChooseSectionOptions)?.Topic
                        ?? throw new ArgumentNullException("step.Options", "Step options must be provided when begining section selection.");

                    step.Values[Values.Topic] = topic;

                    Debug.WriteLine($"Entering >> Dialog >> ChooseSection, step 1 (for topic {topic.Name}).");

                    return await step.PromptAsync(Inputs.Section, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Choose a section:"),
                        RetryPrompt = MessageFactory.Text("Please choose one of these sections, or type `help`."),
                        Choices = ChoiceFactory.ToChoices(topic.Sections.Keys.ToList()),
                    });
                },
                async (step, cancellationToken) =>
                {
                    TopicDescriptor topic = step.Values[Values.Topic] as TopicDescriptor
                        ?? throw new InvalidOperationException("SelectionDialog, step 2 has no Topic value set.");

                    Debug.WriteLine($"Entering >> Dialog >> ChooseSection, step 2 (for topic {topic.Name}).");

                    if (step.Result is Command command)
                    {
                        if (command.Equals(Command.Help))
                        {
                            await step.Context.SendActivityAsync(Responses.Help);
                            return await step.ReplaceDialogAsync(Inputs.ChooseSection, new ChooseSectionOptions { Topic = topic });
                        }
                        else if (command.Equals(Command.Back)
                            || command.Equals(Command.Reset))
                        {
                            // Return to the topic selection dialog.
                            await step.Context.TraceActivityAsync("Exiting the choose section dialog.");
                            return await step.EndDialogAsync();
                        }
                    }
                    else if (step.Result is string section)
                    {
                        Type botType = topic.Sections[section];
                        System.Reflection.ConstructorInfo[] ctors = botType.GetConstructors();
                        IBot bot = null;
                        foreach(System.Reflection.ConstructorInfo ctor in ctors)
                        {
                            System.Reflection.ParameterInfo[] paramInfo = ctor.GetParameters();
                            var parameters = new object[paramInfo.Length];
                            for (int i = 0; i < paramInfo.Length; i++)
                            {
                                parameters[i] = paramInfo[i].DefaultValue;
                            }
                            if (paramInfo.Length is 0
                                || paramInfo[0].IsOptional)
                            {
                                bot = ctor.Invoke(parameters) as IBot;
                                break;
                            }
                            else if ((paramInfo.Length is 1 || paramInfo[1].IsOptional)
                                && paramInfo[0].ParameterType == typeof(ConversationState))
                            {
                                parameters[0] = convState;
                                bot = ctor.Invoke(parameters) as IBot;
                                break;
                            }
                        }
                        RunSnippetOptions options = new RunSnippetOptions
                        {
                            Section = section,
                            Bot = bot ?? throw new InvalidOperationException($"Could not construct a {botType} bot for section {section}."),
                        };

                        await step.Context.TraceActivityAsync($"Starting the run snippet dialog for topic **{topic.Name}**," +
                            $" section **{section}** (`{options.Bot.GetType().Name}`).");
                        return await step.BeginDialogAsync(Inputs.RunSnippet, options);
                    }

                    // else repeat, using the same initial state, that is, for the same topic.
                    // shouldn't really get here.
                    return await step.ReplaceDialogAsync(
                        Inputs.ChooseSection,
                        new ChooseSectionOptions { Topic = topic });
                },
                async (step, cancellationToken) =>
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
                            return await step.ReplaceDialogAsync(
                                Inputs.ChooseSection,
                                new ChooseSectionOptions { Topic = topic });
                        }
                        else if (command.Equals(Command.Reset))
                        {
                            // Exit and signal that it because of the reset.
                            return await step.EndDialogAsync(command);
                        }
                        else
                        {
                            // Shouldn't get here, but fail gracefully.
                            await step.Context.TraceActivityAsync(
                                $"Hit SelectionDialog, step 3 with a {command.Name} command. Repeating the dialog over again.");
                            return await step.EndDialogAsync();
                        }
                    }
                    else
                    {
                        // Shouldn't get here, but fail gracefully.
                        await step.Context.TraceActivityAsync(
                            $"Hit SelectionDialog, step 3 with a step.Result of {step.Result ?? "null"}. Repeating the dialog over again.");
                        return await step.EndDialogAsync();
                    }
                },
            }));

            Add(new WaterfallDialog(Inputs.RunSnippet, new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    IBot bot = (step.Options as RunSnippetOptions).Bot;
                    step.Values[Values.Bot] = bot;

                    Debug.WriteLine($"Entering >> Dialog >> RunSnippet, step 1 (for bot {bot.GetType().Name}).");

                    string text = step.Context.Activity.AsMessageActivity().Text?.Trim();
                    if (Command.Help.Equals(text))
                    {
                        await step.Context.SendActivityAsync(Responses.Help);
                        return await step.ReplaceDialogAsync(Inputs.RunSnippet, new RunSnippetOptions { Bot = bot });
                    }
                    else if (Command.Back.Equals(text))
                    {
                        return await step.EndDialogAsync(Command.Back);
                    }
                    else if (Command.Reset.Equals(text))
                    {
                        return await step.EndDialogAsync(Command.Reset);
                    }
                    else
                    {
                        await bot.OnTurnAsync(step.Context);
                        return Dialog.EndOfTurn;
                    }
                },
                async (step, cancellationToken) =>
                {
                    IBot bot = step.Values[Values.Bot] as IBot;

                    Debug.WriteLine($"Entering >> Dialog >> RunSnippet, step 2 (for bot {bot.GetType().Name}).");

                    return await step.ReplaceDialogAsync(Inputs.RunSnippet, new RunSnippetOptions { Bot = bot });
                },
            }));
        }
    }
}
