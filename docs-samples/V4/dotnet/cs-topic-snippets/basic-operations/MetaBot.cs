using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace basicOperations
{
    public class MetaBot : IBot
    {
        /// <summary>Contains the names of the dialogs and prompts.</summary>
        private struct Input
        {
            public const string ChooseTopic = "chooseTopic";
            public const string ChooseSection = "chooseSection";
            public const string RunSnippet = "runSnippet";
            public const string Topic = "topicPrompt";
            public const string Section = "sectionPrompt";
            public const string Run = "passThroughPrompt";
        }

        /// <summary>Contains the keys for step state (the step.Values dictionary).</summary>
        private struct Value
        {
            public const string Topic = "topic";
            public const string Section = "section";
            public const string Bot = "bot";
        }

        /// <summary>Represents a command, a meta-level instruction for controling the selection behavior.</summary>
        public class Command : IEquatable<Command>, IEquatable<string>
        {
            public Command(string name)
            {
                Name = !string.IsNullOrWhiteSpace(name)
                    ? name.Trim().ToLowerInvariant()
                    : throw new ArgumentNullException(nameof(name));
            }

            public string Name { get; }

            public bool Equals(Command other)
            {
                return Name.Equals(other?.Name, StringComparison.InvariantCultureIgnoreCase);
            }

            public bool Equals(string other)
            {
                return Name.Equals(other, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>Defines the available commands.</summary>
        public struct Commands
        {
            /// <summary>The Help command: provide help, repeat the current dialog.</summary>
            public static Command Help { get; } = new Command("help");

            /// <summary>The Back command: exit the current dialog, if not already at the top.</summary>
            public static Command Back { get; } = new Command("back");

            /// <summary>The Reset command: clear the stack and start again from the top.</summary>
            public static Command Reset { get; } = new Command("reset");

            /// <summary>All of the defined commands, as a list.</summary>
            public static IReadOnlyList<Command> List
                = new List<Command> { Help, Back, Reset };
        }

        /// <summary>Contains "canned" response messages.</summary>
        public struct Response
        {
            public static IActivity Welcome { get; }
                = MessageFactory.Text($"Welcome to the Bot101 snippets collection.");
            public static IActivity Error { get; }
                = MessageFactory.Text("I'm sorry, that's not a valid input at this stage.");
            public static IActivity Help { get; }
                = MessageFactory.Text("Type `help` for help, `back` to go back a level, or `reset` to back to topic selection.");
        }

        /// <summary>Represents a topic on the docs site.</summary>
        public class Topic
        {
            public string Name { get; set; }
            public string File { get; set; }

            /// <summary>The bots containing snippet code for topic,
            /// indexed by the section in which they appear.</summary>
            public IDictionary<string, IBot> Sections { get; set; }
        }

        /// <summary>Contains the "topic/section/bot index".</summary>
        public struct Snippets
        {
            /// <summary>List of all topics (and snippets) available via this bot.</summary>
            public static IReadOnlyList<Topic> Topics { get; } = new List<Topic>
            {
                new Topic
                {
                    Name = "Sending messages",
                    File = "bot-builder-howto-send-messages.md",
                    Sections = new Dictionary<string, IBot>
                    {
                        ["Send a simple text message"] = new SendMessages.SimpleText(),
                        ["Send a spoken message"] = new SendMessages.SpokenMessage(),
                    },
                },
                new Topic
                {
                    Name = "Add media to messages",
                    File = "bot-builder-howto-add-media-attachments.md",
                    Sections = new Dictionary<string, IBot>
                    {
                        ["Send attachments, just 1"] = new AddMediaAttachments.AnAttachment(),
                        ["Send attachments, multiple"] = new AddMediaAttachments.Attachments(),
                        ["Send a Hero card"] = new AddMediaAttachments.AHeroCard(),
                        ["Send a Hero card using various event types"] = new AddMediaAttachments.AHeroCardWithEvents(),
                        ["Send an Adaptive Card"] = new AddMediaAttachments.AnAdaptiveCard(),
                        ["Send a carousel of cards"] = new AddMediaAttachments.ACarousel(),
                    },
                },
                new Topic
                {
                    Name = "Add input hints to messages",
                    File = "bot-builder-howto-add-input-hints.md",
                    Sections = new Dictionary<string, IBot>
                    {
                        ["Accepting input"] = new AddInputHints.AcceptingInput(),
                        ["Expecting input"] = new AddInputHints.ExpectingInput(),
                        ["Ignoring input"] = new AddInputHints.IgnoringInput(),
                    },
                },
                new Topic
                {
                    Name = "Add suggested actions to messages",
                    File = "bot-builder-howto-add-suggested-actions.md",
                    Sections = new Dictionary<string, IBot>
                    {
                        ["Send suggested actions"] = new AddSuggestedActions(),
                    },
                },
            /* template code
                new Topic
                {
                    Name = "topicName",
                    File = "fileName.md",
                    Sections = new Dictionary<string, IBot>
                    {
                        ["sectionName"] = new botClass(),
                    },
                },
             **/
            };

            public static IActivity ChooseTopic { get; }
                = MessageFactory.SuggestedActions(Topics.Select(t => t.Name), "Choose a topic:");
        }

        /// <summary>Contains the dialog options for the section selection dialog.</summary>
        public class SectionOptions : DialogOptions
        {
            public Topic Topic { get; set; }
        }

        /// <summary>Contains the dialog options for the run a snippet dialog.</summary>
        public class SnippetOptions : DialogOptions
        {
            public string Section { get; set; }
            public IBot Bot { get; set; }
        }

        private StateAccessors Accessor { get; }

        /// <summary>A dialog set for navigating the topic-section-snippet structure.</summary>
        private DialogSet SelectionDialog { get; }

        /// <summary>Returns either the command entered or the index of the topic selected.</summary>
        /// <param name="context">The turn context.</param>
        /// <param name="prompt">The validation context.</param>
        /// <returns>A task representing the operation to perform.</returns>
        private async Task TopicValidator(
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
                Command command = Commands.List.FirstOrDefault(c => c.Equals(text));
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
        private async Task SectionValidator(
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
                Command command = Commands.List.FirstOrDefault(c => c.Equals(text));
                if (command != null)
                {
                    // Return the command entered.
                    prompt.End(command);
                }
            }
        }

        /// <summary>Creates a new instance of the bot.</summary>
        /// <param name="accessor">The state property accessors for the bot.</param>
        public MetaBot(StateAccessors accessor)
        {
            Accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            SelectionDialog = new DialogSet(Accessor.SelectionDialogState);

            SelectionDialog.Add(new ChoicePrompt(Input.Topic, TopicValidator, defaultLocale: Culture.English));
            SelectionDialog.Add(new ChoicePrompt(Input.Section, SectionValidator, defaultLocale: Culture.English));
            SelectionDialog.Add(new TextPrompt(Input.Run));
            SelectionDialog.Add(new WaterfallDialog(Input.ChooseTopic, new WaterfallStep[]
            {
                async (dc, step, cancellationToken) =>
                {
                    return await dc.PromptAsync(Input.Topic, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Choose a topic:"),
                        RetryPrompt = MessageFactory.Text("Please choose one of these topics, or type `help`."),
                        Choices = ChoiceFactory.ToChoices(Snippets.Topics.Select(t => t.Name).ToList()),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    if (step.Result is Command command)
                    {
                        if (command.Equals(Commands.Help))
                        {
                                await dc.Context.SendActivityAsync(Response.Help);
                        }

                        // All other commands are no-ops, as we're already at the "top level".
                        await dc.Context.TraceActivityAsync("ChooseTopic, step 3: Repeating the choose topic dialog.");
                        return await dc.ReplaceAsync(Input.ChooseTopic);
                    }
                    else if (step.Result is int index)
                    {
                        SectionOptions sectionOptions = new SectionOptions { Topic = Snippets.Topics[index] };
                        await dc.Context.TraceActivityAsync($"Selected topic **{sectionOptions.Topic.Name}**.");
                        return await dc.BeginAsync(Input.ChooseSection, sectionOptions);
                    }

                    // else, we shouldn't get here, but fail gracefully.
                    await dc.Context.TraceActivityAsync("ChooseTopic, step 2, graceful fail: Repeating the choose topic dialog.");
                    return await dc.ReplaceAsync(Input.ChooseTopic);
                },
                async (dc, step, cancellationToken) =>
                {
                    // We're resurfacing from the select-section dialog.
                    // This is the top level, so we don't really care how things bubbled back up.
                    await dc.Context.TraceActivityAsync("ChooseTopic, step 3: Repeating the choose topic dialog.");
                    return await dc.ReplaceAsync(Input.ChooseTopic);
                },
            }));
            SelectionDialog.Add(new WaterfallDialog(Input.ChooseSection, new WaterfallStep[]
            {
                async (dc, step, cancellationToken) =>
                {
                    Topic topic = (step.Options as SectionOptions)?.Topic
                        ?? throw new ArgumentNullException("step.Options", "Step options must be provided when begining section selection.");
                    step.Values[Value.Topic] = topic;
                    return await dc.PromptAsync(Input.Section, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Choose a section:"),
                        RetryPrompt = MessageFactory.Text("Please choose one of these sections, or type `help`."),
                        Choices = ChoiceFactory.ToChoices(topic.Sections.Keys.ToList()),
                    });
                },
                async (dc, step, cancellationToken) =>
                {
                    Topic topic = step.Values[Value.Topic] as Topic
                        ?? throw new InvalidOperationException("SelectionDialog, step 2 has no Topic value set.");

                    if (step.Result is Command command)
                    {
                        if (command.Equals(Commands.Help))
                        {
                            await dc.Context.SendActivityAsync(Response.Help);
                            return await dc.ReplaceAsync(Input.ChooseSection, new SectionOptions { Topic = topic });
                        }
                        else if (command.Equals(Commands.Back)
                            || command.Equals(Commands.Reset))
                        {
                            // Return to the topic selection dialog.
                            await dc.Context.TraceActivityAsync("Exiting the choose section dialog.");
                            return await dc.EndAsync();
                        }
                    }
                    else if (step.Result is string section)
                    {
                        SnippetOptions options = new SnippetOptions { Bot = topic.Sections[section] };
                        await dc.Context.TraceActivityAsync($"Starting the run snippet dialog for topic **{topic.Name}**," +
                            $" section **{section}** (`{options.Bot.GetType().Name}`).");
                        return await dc.BeginAsync(Input.RunSnippet, options);
                    }

                    // else repeat, using the same initial state, that is, for the same topic.
                    // shouldn't really get here.
                    return await dc.ReplaceAsync(
                        Input.ChooseSection,
                        new SectionOptions { Topic = topic });
                },
                async (dc, step, cancellationToken) =>
                {
                    Topic topic = step.Values[Value.Topic] as Topic
                        ?? throw new InvalidOperationException("SelectionDialog, step 3 has no Topic value set.");

                    // We're resurfacing from the run-snippet dialog.
                    // Should only be via a back or reset command.
                    if (step.Result is Command command)
                    {
                        if (command.Equals(Commands.Back))
                        {
                            // Repeat, using the same initial state, that is, for the same topic.
                            return await dc.ReplaceAsync(
                                Input.ChooseSection,
                                new SectionOptions { Topic = topic });
                        }
                        else if (command.Equals(Commands.Reset))
                        {
                            // Exit and signal that it because of the reset.
                            return await dc.EndAsync(command);
                        }
                        else
                        {
                            // Shouldn't get here, but fail gracefully.
                            await dc.Context.TraceActivityAsync($"Hit SelectionDialog, step 3 with a {command.Name} command. Repeating the dialog over again.");
                            return await dc.EndAsync();
                        }
                    }
                    else
                    {
                        // Shouldn't get here, but fail gracefully.
                        await dc.Context.TraceActivityAsync($"Hit SelectionDialog, step 3 with a step.Result of {step.Result ?? "null"}. Repeating the dialog over again.");
                        return await dc.EndAsync();
                    }
                },
            }));
            SelectionDialog.Add(new WaterfallDialog(Input.RunSnippet, new WaterfallStep[]
            {
                async (dc, step, cancellationToken) =>
                {
                    IBot bot = (step.Options as SnippetOptions).Bot;
                    step.Values[Value.Bot] = bot;
                    string text = dc.Context.Activity.AsMessageActivity().Text?.Trim();
                    if (Commands.Help.Equals(text))
                    {
                        await dc.Context.SendActivityAsync(Response.Help);
                        return await dc.ReplaceAsync(Input.RunSnippet, new SnippetOptions { Bot = bot });
                    }
                    else if (Commands.Back.Equals(text))
                    {
                        return await dc.EndAsync(Commands.Back);
                    }
                    else if (Commands.Reset.Equals(text))
                    {
                        return await dc.EndAsync(Commands.Reset);
                    }
                    else
                    {
                        await bot.OnTurnAsync(dc.Context);
                        return Dialog.EndOfTurn;
                    }
                },
                async (dc, step, cancellationToken) =>
                {
                    IBot bot = step.Values[Value.Bot] as IBot;
                    return await dc.ReplaceAsync(Input.RunSnippet, new SnippetOptions { Bot = bot });
                },
            }));
        }

        public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
        {
            //TopicState state = await Accessor.TopicState.GetAsync(context);
            DialogContext dc = await SelectionDialog.CreateContextAsync(context);
            switch (context.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:

                    IConversationUpdateActivity update = context.Activity.AsConversationUpdateActivity();
                    if (update.MembersAdded.Any(m => m.Id != update.Recipient.Id))
                    {
                        await dc.BeginAsync(Input.ChooseTopic);
                    }

                    break;

                case ActivityTypes.Message:

                    DialogTurnResult turnResult = await dc.ContinueAsync();

                    break;
            }
        }
    }
}
