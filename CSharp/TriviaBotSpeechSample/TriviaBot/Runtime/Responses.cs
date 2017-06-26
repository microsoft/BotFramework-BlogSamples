// Copyright (c) Microsoft Corporation. All rights reserved.

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TriviaBot.Shared;

namespace TriviaBot.Runtime
{
    /// <summary>
    /// Contains all of the reponses from the bot to the user.
    /// This separates content from the core bot logic, and allows reuse.
    /// </summary>
    public static class Responses
    {
        public async static Task Send_LetsGo(IDialogContext context, IMessageActivity message)
        {
            var reply = CreateResponse(
                            context,
                            message,
                            "Let's go!",
                            "Let's go!",
                            messageType: MessageType.Statement,
                            inputHint: InputHints.IgnoringInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_Goodbye(IDialogContext context, IMessageActivity message)
        {
            var reply = CreateResponse(
                            context,
                            message,
                            "Goodbye.",
                            "See you next time!",
                            messageType: MessageType.Statement);

            await context.PostAsync(reply);
        }

        public async static Task Send_TimeForNextQuestion(IDialogContext context, IMessageActivity message)
        {
            var replyText = SelectRandomString(new string[] { "Alright, here's the next one",
                                                              "Here's one more", $"Ok, next question",
                                                              "Time for the next one" });

            var reply = CreateResponse(
                            context,
                            message,
                            displayText: replyText + ":",
                            speakText: replyText,
                            messageType: MessageType.Statement,
                            inputHint: InputHints.IgnoringInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_Greeting(IDialogContext context, IMessageActivity message)
        {
            var reply = CreateResponse(
                            context,
                            message,
                            "Welcome to Trivia!",
                            "Welcome to Trivia! To stop playing you can say. Cancel, or, I'm done playing, at any time. Now, Let's get started.",
                            audioToPlay: "a_sunshine_intro_09.wav",
                            messageType: MessageType.Statement,
                            inputHint: InputHints.IgnoringInput);

            if(message.ChannelId?.ToLower() != "cortana")
            { 
                reply.Attachments.Add((new ThumbnailCard()
                {
                    Images = new[] { new CardImage("https://trivasdkbot.azurewebsites.net/Assets/trivia.png") },
                }).ToAttachment());
            }

            await context.PostAsync(reply);
        }

        public async static Task Send_FirstQuestion(IDialogContext context, IMessageActivity message)
        {
            var reply = CreateResponse(
                        context,
                        message,
                        "Here's your first question:",
                        "Here's your first question.",
                        messageType: MessageType.Statement,
                        inputHint: InputHints.IgnoringInput);

            await context.PostAsync(reply);
        }

        public async static Task SendQuestion(IDialogContext context, IMessageActivity message, string question, IList<string> options)
        {
            // Send Question & possible answers
            var reply = CreateResponse(
                            context,
                            message,
                            question,
                            question,
                            messageType: MessageType.Question,
                            optionsToAdd: options,
                            inputHint: InputHints.ExpectingInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_DidNotUnderstand(IDialogContext context, IMessageActivity message)
        {
            var reply = CreateResponse(
                            context,
                            message,
                            "Sorry, I don't understand that.",
                            "Sorry, I don't understand that.",
                            messageType: MessageType.Statement,
                            inputHint: InputHints.IgnoringInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_WhichOne(IDialogContext context, IMessageActivity message, IList<string> options)
        {
            var replyText = "Which one did you mean?";

            var reply = CreateResponse(
                            context,
                            message,
                            replyText,
                            replyText,
                            messageType: MessageType.Question,
                            optionsToAdd: options,
                            inputHint: InputHints.ExpectingInput);

            System.Diagnostics.Trace.WriteLine("message: " + Newtonsoft.Json.JsonConvert.SerializeObject(message));
            System.Diagnostics.Trace.WriteLine("reply: " + Newtonsoft.Json.JsonConvert.SerializeObject(reply));

            await context.PostAsync(reply);
        }

        public async static Task AskForCategory(IDialogContext context, IMessageActivity message)
        {
            var replyText = "What category would you like to switch to?";

            var reply = CreateResponse(
                            context,
                            message,
                            replyText,
                            replyText,
                            messageType: MessageType.Statement,
                            inputHint: InputHints.ExpectingInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_ConfirmAnswer(IDialogContext context, IMessageActivity message, string choice)
        {
            var confirmationReply = CreateResponse(
                                        context,
                                        message,
                                        $"Is {choice} your final answer?",
                                        $"I think you said {choice}. Is that your final answer?",
                                        messageType: MessageType.Statement,
                                        inputHint: InputHints.ExpectingInput);

            await context.PostAsync(confirmationReply);
        }

        public async static Task Send_CorrectAnswer(IDialogContext context, IMessageActivity message, string correctAnswer)
        {
            var replyText = SelectRandomString(new string[] { "Congrats, you got it right!",
                                                              "Correct!", $"That's right, the answer is {correctAnswer}",
                                                              "100% correct!" });

            var reply = CreateResponse(
                            context,
                            message,
                            replyText,
                            replyText,
                            messageType: MessageType.GotItRight,
                            audioToPlay: "tv_gameshow_bell_01.wav",
                            inputHint: InputHints.IgnoringInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_OutOfTime_First(IDialogContext context, IMessageActivity message)
        {
            var replyText = SelectRandomString(new string[] { "Oops! You ran out of time!",
                                                              "Come on, you've gotta be faster than that!",
                                                              "You don't have to be fast, but you can't be that slow!",
                                                              "I don't have all day!" });
            var reply = CreateResponse(
                            context,
                            message,
                            replyText,
                            replyText,
                            messageType: MessageType.OutOfTime,
                            inputHint: InputHints.IgnoringInput);
            
            await context.PostAsync(reply);
        }

        public async static Task Send_OutOfTime_X(IDialogContext context, IMessageActivity message)
        {
            var replyText = SelectRandomString(new string[] { "Did you forget you were playing? Say something to continue.",
                                                              "Fine, I won't talk to you either! Say something to continue.",
                                                              "You don't have to be fast, but you can't be that slow! Say something to continue.",
                                                              "Again?  Say something to continue." });
            var reply = CreateResponse(
                            context,
                            message,
                            replyText,
                            replyText,
                            messageType: MessageType.OutOfTime,
                            inputHint: InputHints.AcceptingInput);


            await context.PostAsync(reply);
        }

        public async static Task Send_TryEasierQuestion(IDialogContext context, IMessageActivity message)
        {
            var replyText = SelectRandomString(new string[] { "Let's try something a little easier this time...",
                                                              "Maybe this one will be better.",
                                                              "I believe in you, let's keep going!",
                                                              "Let's try a different question this time." });
            var reply = CreateResponse(
                                        context,
                                        message,
                                        replyText,
                                        replyText,
                                        messageType: MessageType.Statement,
                                        inputHint: InputHints.IgnoringInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_IncorrectAnswer(IDialogContext context, IMessageActivity message, string correctAnswer)
        {
            var replyText = SelectRandomString(new string[] { $"Sorry, the correct answer was: {correctAnswer}",
                                                              $"Not quite, the correct answer was: {correctAnswer}",
                                                              $"That's incorrect. The answer was: {correctAnswer}" });

            var reply = CreateResponse(
                            context,
                            message,
                            replyText,
                            replyText,
                            messageType: MessageType.GotItWrong,
                            audioToPlay: "tv_gameshow_buzzer_03.wav",
                            inputHint: InputHints.IgnoringInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_Help_Game(IDialogContext context, IMessageActivity message)
        {
            var reply = CreateResponse(
                            context,
                            message,
                            "Here are some things you can say to me: \"I'd like to play a game\", \"Let's play trivia\"",
                            "Here are some things you can say to me: \"I'd like to play a game\", \"Let's play trivia\"",
                            messageType: MessageType.Statement,
                            inputHint: InputHints.AcceptingInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_Help_Trivia(IDialogContext context, IMessageActivity message)
        {
            var reply = CreateResponse(
                            context,
                            message,
                            "Here are some things you can say to me: \"I'd like to play trivia\", \"How about trivia\"",
                            "Here are some things you can say to me: \"I'd like to play trivia\", \"How about trivia\"",
                            messageType: MessageType.Statement,
                            inputHint: InputHints.AcceptingInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_Help_Trivia(IDialogContext context, IMessageActivity message, IList<string> options)
        {
            var replyText = "You're playing trivia! Here are your choices:";

            var reply = CreateResponse(
                            context,
                            message,
                            replyText,
                            replyText,
                            messageType: MessageType.Statement,
                            optionsToAdd: options,
                            inputHint: InputHints.ExpectingInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_Error_UnknownState(IDialogContext context, IMessageActivity message)
        {
            var reply = CreateResponse(
                            context,
                            message,
                            "Sorry, I seem to have gotten into a bad state. I'll try resetting the conversation.",
                            "Sorry, I seem to have gotten into a bad state. I'll try resetting the conversation.",
                            messageType: MessageType.Statement,
                            inputHint: InputHints.AcceptingInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_Error_NextQuestion(IDialogContext context, IMessageActivity message)
        {
            var errorReply = CreateResponse(
                                context,
                                message,
                                "Sorry, an error occurred. Here's another question.",
                                "I seem to have hit a snag, let's try another question.",
                                messageType: MessageType.Statement,
                                inputHint: InputHints.IgnoringInput);

            await context.PostAsync(errorReply);
        }

        public async static Task Send_PlayingCategory(IDialogContext context, IMessageActivity message, TriviaCategory category)
        {
            var replyText = SelectRandomString(new string[] { $"We're playing: \"{category.DisplayName()}\".",
                                                              $"We're going with: \"{category.DisplayName()}\" today.",
                                                              $"You'll get questions about: \"{category.DisplayName()}\"." });

            var reply = CreateResponse(
                            context,
                            message,
                            replyText,
                            replyText,
                            messageType: MessageType.Statement,
                            inputHint: InputHints.IgnoringInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_SwitchedCategory(IDialogContext context, IMessageActivity message, TriviaCategory category)
        {
            var replyText = $"You've switched to the category \"{category.DisplayName()}\"";

            var reply = CreateResponse(
                            context,
                            message,
                            replyText,
                            replyText,
                            messageType: MessageType.Statement,
                            inputHint: InputHints.IgnoringInput);

            await context.PostAsync(reply);
        }

        public async static Task Send_LightningModeStart(IDialogContext context, IMessageActivity message)
        {
            if (message.ChannelId.NormalizedEquals("directline"))
            {
                var replyText = "Starting lightning mode!";

                var reply = CreateResponse(
                                context,
                                message,
                                replyText,
                                replyText,
                                messageType: MessageType.StartLightningMode,
                                inputHint: InputHints.IgnoringInput);

                await context.PostAsync(reply);
            }
            else if (message.ChannelId.NormalizedEquals("cortana"))
            {
                var replyText = "You can play lightning mode in our app! I'll bring you there...";

                var reply = CreateResponse(
                                context,
                                message,
                                replyText,
                                replyText,
                                messageType: MessageType.Statement,
                                inputHint: InputHints.IgnoringInput);

                await context.PostAsync(reply);

                reply = CreateResponse(
                            context,
                            message,
                            null,
                            null,
                            messageType: MessageType.StartLightningMode,
                            inputHint: InputHints.IgnoringInput);

                await context.PostAsync(reply);

                reply.ChannelData = JObject.FromObject(new { action = new { type = "LaunchUri", uri = "triviaapp://play/gameshow" } });

                await context.PostAsync(reply);
            }
            else
            {
                var replyText = "Sorry, Lightning Mode is only supported in our app.";

                var reply = CreateResponse(
                                context,
                                message,
                                replyText,
                                replyText,
                                messageType: MessageType.Statement,
                                inputHint: InputHints.IgnoringInput);

                await context.PostAsync(reply);
            }
        }

        public async static Task Send_LightningModeEnd(IDialogContext context, IMessageActivity message)
        {
            if (message.ChannelId.NormalizedEquals("directline"))
            {
                var replyText = "Turning off lightning mode.";

                var reply = CreateResponse(
                                context,
                                message,
                                replyText,
                                replyText,
                                messageType: MessageType.StopLightningMode,
                                inputHint: InputHints.IgnoringInput);

                await context.PostAsync(reply);
            }
            else
            {
                var replyText = "Sorry, Lightning Mode is only supported in our app. <insert protocol launch button here>";

                var reply = CreateResponse(
                                context,
                                message,
                                replyText,
                                replyText,
                                messageType: MessageType.Statement,
                                inputHint: InputHints.IgnoringInput);

                await context.PostAsync(reply);
            }
        }

        private static IMessageActivity CreateResponse
        (
            IDialogContext context,
            IMessageActivity message,
            string displayText,
            string speakText,
            MessageType messageType,
            string audioToPlay = null,
            IList<string> optionsToAdd = null,
            string inputHint = InputHints.AcceptingInput
        )
        {
            var activityToSend = context.MakeMessage();

            if (displayText != null)
            {
                activityToSend.Text = displayText;
            }

            var ssml = (string)null;
            if (speakText != null)
            {
                var escapedSpeakText = speakText.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
                ssml = SsmlWrapper.Wrap(escapedSpeakText);

                if (audioToPlay != null && message.ChannelId != "cortana")
                {
                    var assetPath = "http://" + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + "/Assets/";
                    var uri = new Uri(assetPath + audioToPlay);
                    ssml = CombineAudioAndTextForSSML(uri, ssml);
                }
            }

            activityToSend.Speak = ssml;
            activityToSend.InputHint = inputHint;

            var appEntities =
                new AppEntities
                {
                    MessageType = messageType,
                    TriviaAnswerOptions = optionsToAdd?.Count > 0 ? optionsToAdd : null
                };

            if (optionsToAdd != null)
            {
                List<CardAction> cardButtons = new List<CardAction>();

                bool numberOptions = optionsToAdd.Count > 2;
                for (int i = 0; i < optionsToAdd.Count; i++)
                {
                    var display = numberOptions ? $"{(i + 1).ToString()}: {optionsToAdd[i]}" : optionsToAdd[i];
                    cardButtons.Add(new CardAction() { Value = optionsToAdd[i], Type = "postBack", Title = display });
                }

                var plCard = new ThumbnailCard()
                {
                    // Title = "Pick an answer",
                    Buttons = cardButtons,
                };
                activityToSend.Attachments.Add(plCard.ToAttachment());
            }

            var entity = new Entity();
            entity.SetAs<AppEntities>(appEntities);
            activityToSend.Entities.Add(entity);

            return activityToSend;
        }

        public static string GetInnerSsmlContents(string ssml)
        {
            StringBuilder sb = new StringBuilder();
            XmlReader reader = null;
            reader = XmlReader.Create(new StringReader(ssml));
            string inner = "";
            if (reader.Read())
            {
                inner = reader.ReadInnerXml();
            }

            return inner;
        }

        public static string CombineAudioAndTextForSSML(Uri audioStream, string text)
        {
            StringBuilder sb = new StringBuilder();
            const string ssmlPrefix = @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' " +
                                            "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                                            "xsi:schemaLocation='http://www.w3.org/2001/10/synthesis " +
                                            "http://www.w3.org/TR/speech-synthesis/synthesis.xsd' " +
                                            "xml:lang='en-us'>";
            const string ssmlSuffix = "</speak>";

            sb.Append(ssmlPrefix);
            sb.Append($"<audio src='{audioStream.AbsoluteUri}'/>");
            sb.Append(GetInnerSsmlContents(SsmlWrapper.Wrap(text)));
            sb.Append(ssmlSuffix);
            return sb.ToString();
        }

        private static string SelectRandomString(IList<string> options)
        {
            var rand = new Random();
            var index = rand.Next(0, options.Count - 1);
            return options[index];
        }
    }
}