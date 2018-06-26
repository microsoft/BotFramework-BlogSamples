using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;

namespace StoreListBot
{
    // In the constructor initialize file storage
    public class EchoBot : IBot
    {
        private readonly FileStorage _myStorage;

        public EchoBot()
        {
            _myStorage = new FileStorage(System.IO.Path.GetTempPath());
            
        }

        // Add a class for storing a log of utterances (text of messages) as a list
        public class UtteranceLog : IStoreItem
        {
            // A list of things that users have said to the bot
            public List<string> UtteranceList { get; private set; } = new List<string>();

            // The number of conversational turns that have occurred        
            public int TurnNumber { get; set; } = 0;

            public string eTag { get; set; } = "*";
        }

        // Replace the OnTurn in EchoBot.cs with the following:
        public async Task OnTurn(ITurnContext context)
        {
            var activityType = context.Activity.Type;

            await context.SendActivity($"Activity type: {context.Activity.Type}.");

            if (activityType == ActivityTypes.Message)
            {
                // *********** begin (create or add to log of messages)
                var utterance = context.Activity.Text;
                bool restartList = false;

                if (utterance.Equals("restart"))
                {
                    restartList = true;
                }

                // Attempt to read the existing property bag
                UtteranceLog logItems = null;
                try
                {
                    logItems = _myStorage.Read<UtteranceLog>("UtteranceLog").Result?.FirstOrDefault().Value;
                }
                catch (System.Exception ex)
                {
                    await context.SendActivity(ex.Message);
                }

                // If the property bag wasn't found, create a new one
                if (logItems is null)
                {
                    try
                    {
                        // add the current utterance to a new object.
                        logItems = new UtteranceLog();
                        logItems.UtteranceList.Add(utterance);

                        await context.SendActivity($"The list is now: {string.Join(", ", logItems.UtteranceList)}");

                        var changes = new KeyValuePair<string, object>[]
                        {
                        new KeyValuePair<string, object>("UtteranceLog", logItems)
                        };
                        await _myStorage.Write(changes);
                    }
                    catch (System.Exception ex)
                    {
                        await context.SendActivity(ex.Message);
                    }
                }
                // logItems.ContainsKey("UtteranceLog") == true, we were able to read a log from storage
                else
                {
                    // Modify its property
                    if (restartList)
                    {
                        logItems.UtteranceList.Clear();
                    }
                    else
                    {
                        logItems.UtteranceList.Add(utterance);
                        logItems.TurnNumber++;
                    }

                    await context.SendActivity($"The list is now: {string.Join(", ", logItems.UtteranceList)}");

                    var changes = new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("UtteranceLog", logItems)
                    };
                    await _myStorage.Write(changes);
                }
            }

            return;
        }
    }
}
