using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Client.SpeechRecognition;
using Microsoft.Bot.Client.SpeechSynthesis;
using Microsoft.Bot.Connector.DirectLine;
using TrivaApp.ViewModels;
using TriviaBot.Shared;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace TrivaApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string IdleSuggestionText = "Type or tap mic and speak.";

        private const string ListeningText = "Listening...";

        private const string SpeechRecognitionTriggerPhrase = "hey trivia bot";

        private const string BotNameTag = "🤖";

        private const string UserNameTag = "👤";

        private Microsoft.Bot.Client.BotClient _botClient = null;

        private bool _isListening;

        private bool _textAddedToBottom;

        private Task _startConversationTask;

        private ObservableCollection<ChatCard> _chatCards = new ObservableCollection<ChatCard>();

        private ObservableCollection<AnswerCard> _answerCards = new ObservableCollection<AnswerCard>();

        private CountdownTimer _countdownTimer = new CountdownTimer();

        private bool _startCountdownTimer;

        private int _wrongAnswers;

        private int _rightAnswers;

        private bool _lightningMode = false;

        public MainPage()
        {
            this.InitializeComponent();

            var windowsSpeechRecognizer = new WindowsSpeechRecognizer();

            // Create the client. By default, it will poll the REST endpoint provided by the direct line, but optionally, we can give it a websocket implementation to use
            _botClient = new Microsoft.Bot.Client.BotClient(BotConnection.DirectLineSecret, BotConnection.ApplicationName)
            {
                // Use the speech synthesizer implementation in the WinRT Windows.Media.SpeechSynthesis namespace
                // Any voice supported by the API can be used. See this page as a reference: https://docs.microsoft.com/en-us/azure/cognitive-services/speech/api-reference-rest/bingvoiceoutput
                // The Built-in Windows speech synthesizer can be used here as an alternative, for a free solution:
                // SpeechSynthesizer = new WindowsSpeechSynthesizer(),
                SpeechSynthesizer = new CognitiveServicesSpeechSynthesizer(BotConnection.BingSpeechKey, Microsoft.Bot.Client.SpeechSynthesis.CognitiveServices.VoiceNames.Jessa_EnUs),

                // Use the Cognitive Services Speech-To-Text API, with speech priming support, as the speech recognizer
                // The Built-in WindowsSpeechRecognizer can be used here as an alternative, for a free solution:
                // SpeechRecognizer = windowsSpeechRecognizer,
                SpeechRecognizer = new CognitiveServicesSpeechRecognizer(BotConnection.BingSpeechKey),

                // Give us the ability to trigger speech recognition on keywords
                // The WindowsMediaSpeechRecognizer can also be used as the primary speech recognizer, instead of CognitiveServicesSpeechRecognizer (above)
                // for a free solution.
                TriggerRecognizer = windowsSpeechRecognizer
            };

            // Attach to the callbacks the client provides for observing the state of the bot
            // This will be called every time the bot sends down an activity
            _botClient.ConversationUpdated += OnConversationUpdated;

            // Speech-related events
            _botClient.SpeechRecognitionStarted += OnSpeechRecognitionStarted;
            _botClient.IntermediateSpeechRecognitionResultReceived += OnIntermediateSpeechRecognitionResultReceived;
            _botClient.SpeechRecognitionEnded += OnSpeechRecognitionEnded;
            _botClient.FinalSpeechRecognitionResultReceived += OnFinalSpeechRecognitionResultReceived;
            _botClient.SpeechSynthesisEnded += OnSpeechSynthesisEnded;

            // Set triggers, so that, when the user says "listen" or "what is" the bot client will start speech recognition
            _botClient.SetStartSpeechRecognitionTriggers(new string[] { "listen", "trivia bot" });

            _countdownTimer.PropertyChanged += UpdateCountdown;

            // Kick off the conversation
            _startConversationTask = _botClient.StartConversation();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && (string)e.Parameter == "gameshow-start")
            {
                _model["GameFormat"] = 0;

                var entities = new List<Microsoft.Bot.Connector.DirectLine.Entity> { new Microsoft.Bot.Connector.DirectLine.Entity("skipIntro") };
                await _botClient.SendMessageToBot("play in lightning mode", entities);
            }
        }

        private ObservableDictionary _model = new ObservableDictionary
        {
            { "GameFormat", 1 },
            { "HypothesisText", "play trivia" },
            { "SuggestionText", IdleSuggestionText },
            { "LastBotMessage", string.Empty },
            { "RemainingTime", "∞" },
            { "EnableUserInput", true },
            { "Score0", "0" },
            { "Score0Label", "INCORRECT" },
            { "Score1", "0" },
            { "Score1Label", "CORRECT" },
            { "ShowHypothesisText", false },
            { "ShowGotItRight", false },
            { "ShowGotItWrong", false },
            { "ShowCategoryAll", true },
            { "ShowCategoryFilm", false },
            { "ShowCategoryAnimals", false },
            { "ShowCategoryScience", false },
            { "ShowCategoryGeography", false },
            { "ShowCategoryMusic", false },
            { "ShowCategoryArt", false }
        };

        private readonly string[] _categories = new string[]
        {
            "All",
            "Film",
            "Animals",
            "Science",
            "Geography",
            "Music",
            "Art"
        };

        public ObservableDictionary DefaultViewModel
        {
            get
            {
                return _model;
            }
        }

        private enum MessageSource
        {
            User,
            Bot
        }

        private IAsyncAction RunOnUi(Windows.UI.Core.DispatchedHandler action)
        {
            return Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, action);
        }

        private async void OnSpeechRecognitionStarted(object sender, EventArgs e) => await RunOnUi(async () =>
        {
            await _botClient.SpeechSynthesizer.StopSpeakingAsync();
            _isListening = true;
            _model["HypothesisText"] = string.Empty;
            _model["SuggestionText"] = ListeningText;
            _model["ShowHypothesisText"] = true;
        });

        private async void OnIntermediateSpeechRecognitionResultReceived(object sender, SpeechRecognitionResultEventArgs e) => await RunOnUi(() =>
        {
            if (e.Result.Status == SpeechRecognitionStatus.Success)
            {
                _model["HypothesisText"] = e?.Result?.Text ?? string.Empty;
            }
        });

        private async void OnSpeechRecognitionEnded(object sender, EventArgs e) => await RunOnUi(() =>
        {
            _model["HypothesisText"] = string.Empty;
            _model["SuggestionText"] = IdleSuggestionText;
            _model["ShowHypothesisText"] = false;
            _isListening = false;
        });

        private async void OnFinalSpeechRecognitionResultReceived(object sender, SpeechRecognitionResultEventArgs e) => await RunOnUi(async () =>
        {
            if (e.Result.Status == SpeechRecognitionStatus.Success && e.Result.Text?.Length > 0)
            {
                AddChatMessage(MessageSource.User, e.Result.Text);
                await _botClient.SendMessageToBot(e.Result.Text);
            }
        });

        private async void OnConversationUpdated(object sender, Activity e) => await RunOnUi(() =>
        {
            var appEntities = (AppEntities)null;

            // Don't do anything for messages this client sent
            if (e.From.Id == _botClient.ClientID)
            {
                return;
            }

            if (e.Entities != null)
            {
                appEntities =
                    (from entity in e.Entities
                     where entity.Type == "AppEntities"
                     select entity).FirstOrDefault()?.GetAs<AppEntities>();

                if (appEntities != null)
                {
                    // Parse message type
                    if (appEntities.MessageType != null)
                    {
                        var messageType = appEntities.MessageType;

                        _model["ShowGotItRight"] = false;
                        _model["ShowGotItWrong"] = false;

                        if (messageType != MessageType.Question)
                        {
                            _answerCards.Clear();
                            UpdateAnswerCards();
                            _countdownTimer.Reset();
                            _model["RemainingTime"] = "∞";

                            switch (messageType)
                            {
                                case MessageType.Statement:
                                    break;
                                case MessageType.GotItRight:
                                    ++_rightAnswers;
                                    _model["Score1"] = _rightAnswers;
                                    _model["ShowGotItRight"] = true;
                                    break;
                                case MessageType.GotItWrong:
                                    ++_wrongAnswers;
                                    _model["Score0"] = _wrongAnswers;
                                    _model["ShowGotItWrong"] = true;
                                    break;
                                case MessageType.StartLightningMode:
                                    _lightningMode = true;
                                    break;
                                case MessageType.StopLightningMode:
                                    _lightningMode = false;
                                    break;
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(e.Text))
            {
                var choicesMessage = string.Empty;

                // Parse choices
                if (appEntities?.MessageType == MessageType.Question)
                {
                    // We've received a question from the bot. Tell the bot client that the user is likely to say one of
                    // these options, so it can specifically listen for them (supported in the Microsoft.Bot.Client.Universal.WindowsMediaSpeechRecognizer)
                    _botClient.ListenFor(appEntities.TriviaAnswerOptions);

                    _answerCards.Clear();
                    var index = 1;
                    foreach (var option in appEntities.TriviaAnswerOptions ?? Enumerable.Empty<string>())
                    {
                        _answerCards.Add(new AnswerCard { Index = $"{index}.", AnswerText = $" {option}" });
                        choicesMessage += $"\n    {index}. {option}";
                        ++index;
                    }
                    UpdateAnswerCards();

                    _startCountdownTimer = true;
                }

                _model["LastBotMessage"] = e.Text;

                AddChatMessage(MessageSource.Bot, e.Text + choicesMessage);
            }
        });

        private void UpdateAnswerCards()
        {
            _model["AnswerCards"] = _answerCards;
            _model["ShowAnswerCards"] = _answerCards.Count > 0;
        }

        private async void OnSpeechSynthesisEnded(object sender, EventArgs e) => await RunOnUi(() =>
        {
            if (_startCountdownTimer)
            {
                if (_lightningMode)
                {
                    _countdownTimer.Start(TimeSpan.FromSeconds(10));
                }
                _startCountdownTimer = false;
            }
        });

        private async void UpdateCountdown(object sender, PropertyChangedEventArgs e)
        {
            var timeLeft = _countdownTimer.RemainingTime;
            if (timeLeft.TotalMilliseconds == 0)
            {
                _countdownTimer.Reset();
                _model["RemainingTime"] = "∞";

                var appEntities = new AppEntities
                {
                    MessageType = MessageType.OutOfTime
                };

                var dlEntity = new Microsoft.Bot.Connector.DirectLine.Entity();
                dlEntity.SetAs(appEntities);

                // Send an entity to the bot to tell it that the user has run out of time.
                await _botClient.SendMessageToBot(null, new[] { dlEntity });
            }
            else
            {
                _model["RemainingTime"] = timeLeft.TotalSeconds.ToString("0.00");
            }
        }

        private void AddChatMessage(MessageSource messageSource, string message)
        {
            _textAddedToBottom = true;

            var newCard = new ChatCard();

            if (messageSource == MessageSource.Bot)
            {
                newCard.Message = BotNameTag + "\n" + message;
                newCard.BorderAlignment = HorizontalAlignment.Left;
            }
            else if (messageSource == MessageSource.User)
            {
                newCard.Message = UserNameTag + "\n" + message;
                newCard.BorderAlignment = HorizontalAlignment.Right;
            }

            _chatCards.Add(newCard);

            _model["ChatCards"] = _chatCards;
        }

        private async void TextBox_KeyPressedEventHandler(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string s = textBox.Text;
                textBox.Text = string.Empty;
                _model["HypothesisText"] = string.Empty;

                if (!string.IsNullOrWhiteSpace(s))
                {
                    AddChatMessage(MessageSource.User, s);

                    // Send the user's message to the bot
                    await _botClient.SendMessageToBot(s);
                }
            }
        }

        private void StackPanel_LayoutUpdated(object sender, object e)
        {
            if (_textAddedToBottom)
            {
                ScrollView.ChangeView(null, double.MaxValue, null);

                _textAddedToBottom = false;
            }
        }

        private void MicButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isListening)
            {
                _botClient.AutoSpeechRecognition = false;
                _botClient.CancelSpeechRecognition();
            }
            else
            {
                _botClient.StartSpeechRecognition();
                _botClient.AutoSpeechRecognition = true;
            }
        }

        private async void AnswerOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = AnswerOptions.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _answerCards.Count)
            {
                _botClient.AutoSpeechRecognition = false;
                await _botClient.CancelSpeechSynthesis();

                var choice = _answerCards[selectedIndex].AnswerText;
                _answerCards.Clear();
                _model["AnswerCards"] = _answerCards;

                AddChatMessage(MessageSource.User, choice);
                await _botClient.SendMessageToBot(choice);
            }
        }

        private async Task SetCategory(string category)
        {
            // If the user clicked a category, stop any kind of speech input/output and just send the
            // category to the bot.
            await _botClient.SpeechRecognizer.CancelRecognitionAsync();
            await _botClient.SpeechSynthesizer.StopSpeakingAsync();

            await _botClient.SendMessageToBot("switch to category " + category);

            foreach (var c in _categories)
            {
                var visibilityFlag = "ShowCategory" + c;
                var visibility = c == category;
                if (!_model.ContainsKey(visibilityFlag)
                    || (bool)_model[visibilityFlag] != visibility)
                {
                    _model[visibilityFlag] = visibility;
                }
            }
        }

        private async void CategoryFilm_Click(object sender, RoutedEventArgs e)
        {
            await SetCategory("Film");
        }

        private async void CategoryAnimals_Click(object sender, RoutedEventArgs e)
        {
            await SetCategory("Animals");
        }

        private async void CategoryScience_Click(object sender, RoutedEventArgs e)
        {
            await SetCategory("Science");
        }

        private async void CategoryAll_Click(object sender, RoutedEventArgs e)
        {
            await SetCategory("All");
        }

        private async void CategoryGeography_Click(object sender, RoutedEventArgs e)
        {
            await SetCategory("Geography");
        }

        private async void CategoryMusic_Click(object sender, RoutedEventArgs e)
        {
            await SetCategory("Music");
        }

        private async void CategoryArt_Click(object sender, RoutedEventArgs e)
        {
            await SetCategory("Art");
        }

        private void LightningModeButton_Click(object sender, RoutedEventArgs e)
        {
            _botClient.CancelSpeechSynthesis();
            _botClient.CancelSpeechRecognition();

            if (_lightningMode)
            {
                _botClient?.SendMessageToBot("stop lightning mode");
            }
            else
            {
                _botClient?.SendMessageToBot("start lightning mode");
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The gameshow view showcases speech input and output,
            // while the chat view showcases a simple chat control.
            // Enable automatic speech interactions only when on the
            // gameshow view.
            UpdateAnswerCards();
            if (ViewPivot.SelectedIndex == 0)
            {
                _botClient.AutoSpeechRecognition = true;
                _botClient.AutoSpeechSynthesis = true;
            }
            else
            {
                _botClient.AutoSpeechRecognition = false;
                _botClient.AutoSpeechSynthesis = false;
                _botClient.CancelSpeechSynthesis();
            }
        }
    }
}
