namespace PrimitivePrompts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public class PrimitivePromptsBot : IBot
    {
        public const string ProfileTopic = "profile";

        /// <summary>
        /// Describes a field in the user profile.
        /// </summary>
        private class UserFieldInfo
        {
            /// <summary>
            /// The ID to use for this field.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// The prompt to use to ask for a value for this field.
            /// </summary>
            public string Prompt { get; set; }

            /// <summary>
            /// Gets the value of the corresponding field.
            /// </summary>
            public Func<UserProfile, string> GetValue { get; set; }

            /// <summary>
            /// Sets the value of the corresponding field.
            /// </summary>
            public Action<UserProfile, string> SetValue { get; set; }
        }

        /// <summary>
        /// The prompts for the user profile, indexed by field name.
        /// </summary>
        private static List<UserFieldInfo> UserFields { get; } = new List<UserFieldInfo>
        {
            new UserFieldInfo {
                Key = nameof(UserProfile.UserName),
                Prompt = "What is your name?",
                GetValue = (profile) => profile.UserName,
                SetValue = (profile, value) => profile.UserName = value,
            },
            new UserFieldInfo {
                Key = nameof(UserProfile.Age),
                Prompt = "How old are you?",
                GetValue = (profile) => profile.Age.HasValue? profile.Age.Value.ToString() : null,
                SetValue = (profile, value) =>
                {
                    if (int.TryParse(value, out int age))
                    {
                        profile.Age = age;
                    }
                },
            },
            new UserFieldInfo {
                Key = nameof(UserProfile.WorkPlace),
                Prompt = "Where do you work?",
                GetValue = (profile) => profile.WorkPlace,
                SetValue = (profile, value) => profile.WorkPlace = value,
            },
        };

        /// <summary>
        /// The state and state accessors for the bot.
        /// </summary>
        private BotAccessors Accessors { get; }

        public PrimitivePromptsBot(BotAccessors accessors)
        {
            Accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is ActivityTypes.Message)
            {
                // Use the state property accessors to get the dialog state and user profile.
                DialogState dialogState = await Accessors.DialogStateAccessor.GetAsync(turnContext, () => new DialogState(), cancellationToken);
                UserProfile userProfile = await Accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile(), cancellationToken);

                // Check whether we need more information.
                if (dialogState.Topic is ProfileTopic)
                {
                    // If we're expecting input, record it in the user's profile.
                    if (dialogState.Prompt != null)
                    {
                        UserFieldInfo field = UserFields.First(f => f.Key.Equals(dialogState.Prompt));
                        field.SetValue(userProfile, turnContext.Activity.Text.Trim());
                    }

                    // Determine which fields are not yet set.
                    List<UserFieldInfo> emptyFields = UserFields.Where(f => f.GetValue(userProfile) is null).ToList();

                    if (emptyFields.Any())
                    {
                        // If all the fields are empty, send a welcome message.
                        if (emptyFields.Count == UserFields.Count)
                        {
                            await turnContext.SendActivityAsync("Welcome new user, please fill out your profile information.");
                        }

                        // We have at least one empty field. Prompt for the next empty field,
                        // and update the prompt flag to indicate which prompt we just sent,
                        // so that the response can be captured at the beginning of the next turn.
                        UserFieldInfo field = emptyFields.First();
                        await turnContext.SendActivityAsync(field.Prompt);
                        dialogState.Prompt = field.Key;
                    }
                    else
                    {
                        // Our user profile is complete!
                        await turnContext.SendActivityAsync($"Thank you, {userProfile.UserName}. Your profile is complete.");
                        dialogState.Prompt = null;
                        dialogState.Topic = null;
                    }
                }
                else if (turnContext.Activity.Text.Trim().Equals("hi", StringComparison.InvariantCultureIgnoreCase))
                {
                    await turnContext.SendActivityAsync($"Hi. {userProfile.UserName}.");
                }
                else
                {
                    await turnContext.SendActivityAsync("Hi. I'm the Contoso cafe bot.");
                }

                // Use the state property accessors to update the dialog state and user profile.
                await Accessors.DialogStateAccessor.SetAsync(turnContext, dialogState, cancellationToken);
                await Accessors.UserProfileAccessor.SetAsync(turnContext, userProfile, cancellationToken);

                // Save any state changes to storage.
                await Accessors.StateSet.SaveAllChangesAsync(turnContext, false, cancellationToken);
            }
        }
    }
}