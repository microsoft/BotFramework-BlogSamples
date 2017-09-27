// Copyright (c) Microsoft Corporation. All rights reserved.

using Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TriviaBot.Shared;

namespace TriviaBot.Runtime
{
    [Serializable]
    public class TriviaDialog : IDialog<object>
    {
        public BotState CurrentState { get; set; } = BotState.None;

        // In-game state
        public TriviaCategory Category = TriviaCategory.None;
        public string ExpectedAnswer = null;
        public List<string> AnswerOptions = null;

        public string CurrentQuestion = null;
        public string ChosenAnswer = null;
        public bool ConfirmingAnswer = false;
        public bool TimedOutLastTime = false;

        private void ResetState()
        {
            CurrentState = BotState.None;

            Category = TriviaCategory.None;
            ExpectedAnswer = null;
            AnswerOptions = null;

            CurrentQuestion = null;
            ChosenAnswer = null;
            ConfirmingAnswer = false;
            TimedOutLastTime = false;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            var LU = (LuisResult)null;

            // We have some text to process, run LUIS to get intent + entities
            if (message.Text != null)
            {
                LU = await QueryLuis.GetIntentAndEntitiesFromLuis(
                    #error You must specify the LUIS endpoint to talk to. You can do this by creating a new LUIS app on http://luis.ai/, importing the included TriviaBotLU.json, and publishing it.
                    "appID", "subscriptionKey",
                    message.Text);
            }
            
            // Process
            if (LU?.Intents?.Max()?.Intent.NormalizedEquals("Cancel") == true)
            {
                await Responses.Send_Goodbye(context, message);
                ResetState();
            }
            else if (CurrentState == BotState.None)
            {
                await State_None(context, message, LU);
            }
            else if (CurrentState == BotState.Trivia)
            {
                await State_Trivia(context, message, LU);
            }
            else if (CurrentState == BotState.SwitchCategory)
            {
                await State_SwitchCategory(context, message, LU);
            }
            else if (CurrentState == BotState.WaitingForReengagement)
            {
                CurrentState = BotState.Trivia;
                TimedOutLastTime = false;

                await Responses.Send_LetsGo(context, message);
                await Trivia_AskQuestion(context, message, true);
            }
            else
            {
                await Responses.Send_Error_UnknownState(context, message);
                ResetState();
            }

            context.Wait(MessageReceivedAsync);
        }

        private async Task State_None(IDialogContext context, IMessageActivity message, LuisResult luis)
        {
            var intent = luis?.Intents?.Max()?.Intent;

            if (intent == null)
            {
                return;
            }
            if (intent.NormalizedEquals("Help"))
            {
                await Responses.Send_Help_Trivia(context, message);
            }
            else if (intent.NormalizedEquals("Play"))
            {
                await StartGame_Trivia(context, message, luis);
            }
            else if (intent.NormalizedEquals("StartMode"))
            {
                await Responses.Send_LightningModeStart(context, message);
                await Trivia_AskQuestion(context, message, true);
            }
            else
            {
                await Responses.Send_DidNotUnderstand(context, message);
                await Responses.Send_Help_Trivia(context, message);
            }
        }

        private async Task State_Trivia(IDialogContext context, IMessageActivity message, LuisResult luis)
        {
            // We need to handle help, etc at a lower level, because the trivia answers might trigger any intent
            await Trivia_ProcessAnswer(context, message, luis);
        }

        private async Task State_SwitchCategory(IDialogContext context, IMessageActivity message, LuisResult luis)
        {
            bool firstTime = false;
            if (CurrentState != BotState.SwitchCategory)
            {
                firstTime = true;
                CurrentState = BotState.SwitchCategory;
            }

            // See if the category was provided with the SwitchCategory intent
            var categoryValue = GetCategoryFromLuis(luis);

            if (categoryValue == TriviaCategory.None)
            {
                categoryValue = GetCategoryValue(message.Text);
            }

            if (categoryValue != TriviaCategory.None)
            {
                Category = categoryValue;

                await Responses.Send_SwitchedCategory(context, message, Category);
                await Trivia_AskQuestion(context, message, true);
                return;
            }

            if (!firstTime)
            {
                await Responses.Send_DidNotUnderstand(context, message);
            }

            await Responses.AskForCategory(context, message);
        }

        private TriviaCategory GetCategoryFromLuis(LuisResult luis)
        {
            if (luis.Entities != null)
            {
                var categories =
                    from entity in luis.Entities
                    where entity.Type.NormalizedEquals("category")
                    select entity.Entity;

                foreach (var category in categories)
                {
                    var value = GetCategoryValue(category);
                    if (value != TriviaCategory.None)
                    {
                        return value;
                    }
                }
            }

            return TriviaCategory.None;
        }

        private TriviaCategory SwitchCategory_ResolveCategory(IDialogContext context, IMessageActivity message, LuisResult luis)
        {
            var resolvedCategory = TriviaCategory.Any;

            if (luis != null && luis.Entities != null)
            {
                var categories =
                    from entity in luis.Entities
                    where entity.Type.NormalizedEquals("categories")
                    select entity.Entity;

                if (categories.Count() > 0)
                {
                    var category = categories.First();
                    resolvedCategory = GetCategoryValue(category);
                }
            }
            else if (resolvedCategory == TriviaCategory.Any && message.Text != null)
            {
                resolvedCategory = GetCategoryValue(message.Text);
            }

            return resolvedCategory;
        }

        private async Task Trivia_ProcessAnswer(IDialogContext context, IMessageActivity message, LuisResult luis)
        {
            if (ExpectedAnswer == null)
            {
                await Responses.Send_Error_NextQuestion(context, message);
                await Trivia_AskQuestion(context, message, true);
            }

            var providedAnswer = message.Text;

            // If no answer was provided, see if the user ran out of time
            if (providedAnswer == null)
            {
                var appEntitiesJson =
                    (from entity in message.Entities
                     where entity.Type.NormalizedEquals("AppEntities")
                     select entity).FirstOrDefault();

                if (appEntitiesJson != null)
                {
                    var appEntities = appEntitiesJson.GetAs<AppEntities>();

                    if (appEntities.Type.NormalizedEquals("AppEntities") && appEntities.MessageType == MessageType.OutOfTime)
                    {
                        if (!TimedOutLastTime)
                        {
                            TimedOutLastTime = true;

                            await Responses.Send_OutOfTime_First(context, message);


                            await Responses.Send_TryEasierQuestion(context, message);

                            await Trivia_AskQuestion(context, message, true);
                            return;
                        }
                        else
                        {
                            await Responses.Send_OutOfTime_X(context, message);
                            CurrentState = BotState.WaitingForReengagement;

                            return;
                        }
                    }
                }
            }

            TimedOutLastTime = false;

            // User referred to answer indirectly (by position), and we asked for confirmation
            if (ConfirmingAnswer)
            {
                var tentativeAnswer = ChosenAnswer;

                ChosenAnswer = null;
                ConfirmingAnswer = false;

                if (luis.Intents.Max().Intent.NormalizedEquals("Agree"))
                {
                    providedAnswer = tentativeAnswer;
                }
                else
                {
                    await Trivia_AskQuestion(context, message, false);
                    return;
                }
            }

            // Try comparing the answer provided

            // No answer provided, just ignore the message
            if (providedAnswer == null || providedAnswer.Length == 0)
            {
                return;
            }

            // Exact match
            if (providedAnswer.NormalizedEquals(ExpectedAnswer))
            {
                await Responses.Send_CorrectAnswer(context, message, ExpectedAnswer);
                await Trivia_NextQuestion(context, message, luis);
                return;
            }

            // If it matches any answer now, it must be wrong
            foreach (var option in AnswerOptions)
            {
                if (option.NormalizedEquals(providedAnswer))
                {
                    await Responses.Send_IncorrectAnswer(context, message, ExpectedAnswer);
                    await Trivia_NextQuestion(context, message, luis);
                    return;
                }
            }

            // Ordinal answer
            if (luis.Intents.Max().Intent.NormalizedEquals("Select") && luis.Entities.Count > 0)
            {
                var selections =
                    from entity in luis.Entities
                    where entity.Type.NormalizedEquals("selection")
                    select entity.Entity;

                if (selections.Count() > 0)
                {
                    var selection = ResolveOrdinalReference(selections.First()) - 1;
                    
                    if (selection >= 0 && selection < AnswerOptions.Count)
                    {
                        ChosenAnswer = AnswerOptions[selection];
                        ConfirmingAnswer = true;

                        await Responses.Send_ConfirmAnswer(context, message, ChosenAnswer);
                        return;
                    }
                }
            }

            // Didn't match any option, see if it's an intent we can handle
            var intent = luis.Intents.Max()?.Intent;

            if (intent?.NormalizedEquals("Help") == true)
            {
                await Responses.Send_Help_Trivia(context, message, AnswerOptions);
            }
            else if (intent?.NormalizedEquals("SwitchCategory") == true)
            {
                await State_SwitchCategory(context, message, luis);
            }
            else if (intent?.NormalizedEquals("StartMode") == true)
            {
                await Responses.Send_LightningModeStart(context, message);
                await Trivia_AskQuestion(context, message, false);
            }
            else if (intent?.NormalizedEquals("StopMode") == true)
            {
                await Responses.Send_LightningModeEnd(context, message);
                await Trivia_AskQuestion(context, message, false);
            }
            else if (intent?.NormalizedEquals("Play") == true)
            {
                await ProcessLuis_Play(context, message, luis);
                await Trivia_AskQuestion(context, message, false);
            }
            else
            {
                await Responses.Send_DidNotUnderstand(context, message);
                await Responses.Send_WhichOne(context, message, AnswerOptions);
            }
        }

        private async Task Trivia_NextQuestion(IDialogContext context, IMessageActivity message, LuisResult luis)
        {
            await Task.Delay(3000);

            await Responses.Send_TimeForNextQuestion(context, message);
            await Trivia_AskQuestion(context, message, true);
        }

        private async Task StartGame_Trivia(IDialogContext context, IMessageActivity message, LuisResult luis)
        {
            bool skipIntro = message.Entities?.Any(x => x.Type.NormalizedEquals("skipIntro")) == true || message.ChannelId.NormalizedEquals("cortana");

            if (!skipIntro)
            {
                await Responses.Send_Greeting(context, message);
            }
                   
            await ProcessLuis_Play(context, message, luis);

            await Responses.Send_FirstQuestion(context, message);

            await Trivia_AskQuestion(context, message, true);
        }

        // Process information sent with the LUIS intent Play
        private async Task ProcessLuis_Play(IDialogContext context, IMessageActivity message, LuisResult luis)
        {
            var categoryValue = GetCategoryFromLuis(luis);
            if (categoryValue != TriviaCategory.None)
            {
                await Responses.Send_PlayingCategory(context, message, categoryValue);
                Category = categoryValue;
            }

            if (luis?.Entities != null)
            {
                var mode =
                    from entity in luis.Entities
                    where entity.Type.NormalizedEquals("mode")
                    select entity;

                if (mode?.Count() > 0)
                {
                    await Responses.Send_LightningModeStart(context, message);
                }
            }
        }

        private async Task Trivia_AskQuestion(IDialogContext context, IMessageActivity message, bool getNewQuestion)
        {
            CurrentState = BotState.Trivia;

            if (getNewQuestion)
            {
                var url = "https://opentdb.com/api.php?amount=1";
                if (Category != TriviaCategory.None)
                {
                    url += "&category=" + (int)Category;
                }

                var res = HttpGet(url);

                var triviaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TriviaResponse>(res);
                triviaResponse.Decode();

                var question = triviaResponse.Questions[0];

                var maxIncorrect = question.IncorrectAnswers.Count();
                // Skype only supports 3 total buttons, so limit incorrect answers
                if (message.ChannelId.NormalizedEquals("skype"))
                {
                    maxIncorrect = Math.Min(maxIncorrect, 2);
                }
                var allAnswers = question.IncorrectAnswers.ToList().GetRange(0, maxIncorrect);

                allAnswers.Add(question.CorrectAnswer);

                CurrentQuestion = question.Question;
                ExpectedAnswer = question.CorrectAnswer;
                AnswerOptions = ShuffleResults(allAnswers);
            }

            await Responses.SendQuestion(context, message, CurrentQuestion, AnswerOptions);
        }

        private TriviaCategory GetCategoryValue(string category)
        {
            var normalizedCategory = category?.Normalize();

            if (normalizedCategory != null)
            {
                if (normalizedCategory.ContainsIgnoreCase(new[] { "any", "whatever", "don't care", "do not care", "all" })) return TriviaCategory.Any;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "general" })) return TriviaCategory.GeneralKnowledge;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "history" })) return TriviaCategory.History; // Must be before Books (story)
                if (normalizedCategory.ContainsIgnoreCase(new[] { "book", "stories", "story" })) return TriviaCategory.EntertainmentBooks;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "film", "movie" })) return TriviaCategory.EntertainmentFilm;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "music", "song" })) return TriviaCategory.EntertainmentMusic;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "musical", "theater", "theatre" })) return TriviaCategory.EntertainmentMusicalsTheatre;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "television", "tv", "tube" })) return TriviaCategory.EntertainmentTelevision;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "videogames", "video games" })) return TriviaCategory.EntertainmentVideoGames;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "boardgames", "board games" })) return TriviaCategory.EntertainmentBoardGames;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "nature", "natural", "science" })) return TriviaCategory.ScienceNature;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "computers", "software" })) return TriviaCategory.ScienceComputers;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "math" })) return TriviaCategory.ScienceMathematics;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "mythology" })) return TriviaCategory.Mythology;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "sport", "athletic" })) return TriviaCategory.Sports;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "geography" })) return TriviaCategory.Geography;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "politics", "government" })) return TriviaCategory.Politics;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "art", "paint", "draw" })) return TriviaCategory.Art;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "celeb", "famous" })) return TriviaCategory.Celebrities;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "animal" })) return TriviaCategory.Animals;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "vehicle", "car", "driv" })) return TriviaCategory.Vehicles;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "comic", "magazine" })) return TriviaCategory.EntertainmentComics;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "gadget", "electronic", "technology" })) return TriviaCategory.ScienceGadgets;
                if (normalizedCategory.ContainsIgnoreCase(new[] { "anime", "manga" })) return TriviaCategory.EntertainmentJapaneseAnimeManga;
            }

            return TriviaCategory.None;
        }

        private static string HttpGet(string URI)
        {
            WebClient client = new WebClient();

            // Add a user agent header in case the 
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Stream data = client.OpenRead(URI);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();

            return s;
        }

        private static int ResolveOrdinalReference(string message)
        {
            var normalizedMessage = message.Normalize();

            if (normalizedMessage != null)
            {
                if (normalizedMessage.ContainsIgnoreCase(new[] { "tenth", "ten", "10" })) return 10;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "first", "1" })) return 1;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "second", "two", "2" })) return 2;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "third", "three", "3" })) return 3;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "fourth", "four", "4" })) return 4;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "fifth", "five", "5" })) return 5;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "sixth", "six", "6" })) return 6;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "seventh", "seven", "7" })) return 7;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "eight", "eight", "8" })) return 8;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "ninth", "nine", "9" })) return 9;
                if (normalizedMessage.ContainsIgnoreCase(new[] { "one" })) return 1; // last to avoid matching "fourth one"
            }

            return -1;
        }

        private List<string> ShuffleResults(IList<string> list)
        {
            // On a true/false, force that order
            if (list.Count == 2 && 
                list.Contains("True") && list.Contains("False"))
            {
                return new List<string>() { "True", "False" };
            }

            var inList = list.ToList();
            var outList = new List<string>();

            var rand = new Random();
            while (inList.Count > 0)
            {
                var index = rand.Next(inList.Count);
                outList.Add(inList[index]);
                inList.RemoveAt(index);
            }

            return outList;
        }
    }
}