using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace ContosoCafeBot
{
    public class TimexResult : PromptResult
    {
        public string[] Resolutions { get; set; }
    }

    public class TimexPrompt : Prompt<TimexResult>
    {
        private TimexPromptImpl _prompt;

        public TimexPrompt(string culture, PromptValidator<TimexResult> validator = null)
        {
            _prompt = new TimexPromptImpl(culture, validator);
        }

        protected override Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (isRetry)
            {
                if (options.RetryPromptActivity != null)
                {
                    return _prompt.Prompt(dc.Context, options.RetryPromptActivity.AsMessageActivity());
                }
                if (options.RetryPromptString != null)
                {
                    return _prompt.Prompt(dc.Context, options.RetryPromptString, options.RetrySpeak);
                }
            }
            else
            {
                if (options.PromptActivity != null)
                {
                    return _prompt.Prompt(dc.Context, options.PromptActivity);
                }
                if (options.PromptString != null)
                {
                    return _prompt.Prompt(dc.Context, options.PromptString, options.Speak);
                }
            }
            return Task.CompletedTask;
        }

        protected override async Task<TimexResult> OnRecognize(DialogContext dc, PromptOptions options)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return await _prompt.Recognize(dc.Context);
        }

        class TimexPromptImpl : BasePrompt<TimexResult>
        {
            private IModel _model;

            public TimexPromptImpl(string culture, PromptValidator<TimexResult> validator)
               : base(validator)
            {
                _model = new DateTimeRecognizer(culture).GetDateTimeModel();
            }

            public override async Task<TimexResult> Recognize(ITurnContext context)
            {
                BotAssert.ContextNotNull(context);
                BotAssert.ActivityNotNull(context.Activity);
                if (context.Activity.Type == ActivityTypes.Message)
                {
                    var message = context.Activity.AsMessageActivity();
                    var results = _model.Parse(message.Text);
                    if (results.Any())
                    {
                        var result = results.First();
                        if (result.Resolution.Any())
                        {
                            var timexResult = new TimexResult
                            {
                                Status = PromptStatus.Recognized
                            };

                            var distinctTimex = new HashSet<string>();
                            foreach (var resolution in result.Resolution)
                            {
                                var values = (List<Dictionary<string, string>>)resolution.Value;
                                foreach (var timex in values.Select(r => r["timex"]))
                                {
                                    distinctTimex.Add(timex);
                                }
                            }

                            timexResult.Resolutions = distinctTimex.ToArray();

                            await Validate(context, timexResult);
                            return timexResult;
                        }
                    }
                }
                return new TimexResult();
            }
        }
    }
}
