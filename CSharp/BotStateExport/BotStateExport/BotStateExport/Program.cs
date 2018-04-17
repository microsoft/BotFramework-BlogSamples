using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    class Options
    {
        [Option('a', "appId", Required = true, HelpText = "Microsoft Application Id for bot")]
        public string AppId { get; set; }

        [Option('b', "botId", Required = true, HelpText = "bot Id")]
        public string BotId { get; set; }

        [Option('c', "connectionString", HelpText = "set Azure Storage table connection string")]
        public string ConnectionString { get; set; }

        [Option('d', "destination", HelpText = "set 'cosmos', 'file', or 'table'")]
        public string Destination { get; set; }

        [Option('f', "fileName", HelpText = "file in which to write exported data")]
        public string FileName { get; set; }

        [Option('k', "key", HelpText = "Cosmos DB key")]
        public string CosmosDbKey { get; set; }

        [Option('p', "password", Required = true, HelpText = "Microsoft password for bot")]
        public string AppPassword { get; set; }

        [Option('s', "stateUrl", Required = true, HelpText = "Url for state store", Default = "https://connector-api-westus.azurewebsites.net")]
        public string StateUrl { get; set; }

        [Option('u', "url",  HelpText = "Cosmos DB Url")]
        public string CosmosDbUrl { get; set; }
    }

    class Program
    {
       
        static void Main(string[] args)
        {
            bool result;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(opts => result = DoExport(opts).Result)
            .WithNotParsed<Options>((errs) =>
            {
                foreach (var err in errs)
                {
                    Console.WriteLine(err.ToString());
                };
            });
        }

        public static readonly HashSet<string> KnownChannelsIds = new HashSet<string>(new[]
        {
            "bing",
            "cortana",
            "directline",
            "email",
            "facebook",
            "groupme",
            "kik",
            "msteams",
            "skype",
            "skypeforbusiness",
            "slack",
            "sms",
            "telegram",
            "webchat",
            "wechat"
        });

        public static async Task<bool> DoExport(Options opts)
        {
            IBotDataStore<BotData> targetStore = null;
            StreamWriter outputFile = null;
            switch (opts.Destination.ToLower())
            {
                case "cosmos":
                    if (string.IsNullOrEmpty(opts.CosmosDbKey) || string.IsNullOrEmpty(opts.CosmosDbUrl))
                    {
                        Console.WriteLine("Both CosmosDb Key and Url are required in order to copy to a new database");
                        return false;
                    }
                    try
                    {
                        targetStore = new Microsoft.Bot.Builder.Azure.DocumentDbBotDataStore(new Uri(opts.CosmosDbUrl), opts.CosmosDbKey);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Problem initializing cosmos DB: {e}");
                        throw;
                    }
                    break;
                case "file":
                    if (string.IsNullOrEmpty(opts.FileName))
                    {
                        Console.WriteLine("FileName must be specified.");
                        return false;
                    }
                    try
                    {
                        outputFile = new StreamWriter(opts.FileName);
                    }
                    catch
                    {
                        Console.WriteLine($"Error creating output file {opts.FileName}.");

                    }
                    outputFile.WriteLine("{\r\n");
                    break;
                case "table":
                    if (string.IsNullOrEmpty(opts.ConnectionString))
                    {
                        Console.WriteLine("ConnectionString must be set for Azure Storage Table");
                        return false;
                    }

                    try
                    {
                        targetStore = new Microsoft.Bot.Builder.Azure.TableBotDataStore(opts.ConnectionString);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Problem initializing Azure Storage Table: {e}");
                        throw;
                    }
                    break;
                default:
                    Console.WriteLine($"undefined destination type: {opts.Destination}");
                    break;

            }
            if (opts.Destination.ToLower() == "cosmos")
            {
                if (string.IsNullOrEmpty(opts.CosmosDbKey) || string.IsNullOrEmpty(opts.CosmosDbUrl))
                {
                    Console.WriteLine("Both CosmosDb Key and Url are required in order to copy to a new database");
                    return false;     
                }
                try
                {
                    targetStore = new Microsoft.Bot.Builder.Azure.DocumentDbBotDataStore(new Uri(opts.CosmosDbUrl), opts.CosmosDbKey);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Problem initializing cosmos DB: {e}");
                    throw; 
                }
               
            }
            else if (opts.Destination.ToLower() == "table")
            {
                if (string.IsNullOrEmpty(opts.ConnectionString) )
                {
                    Console.WriteLine("ConnectionString must be set for Azure Storage Table");
                    return false;
                }

                try
                {
                    targetStore = new Microsoft.Bot.Builder.Azure.TableBotDataStore(opts.ConnectionString);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Problem initializing Azure Storage Table: {e}");
                    throw;
                }
            }


            var credentials = new MicrosoftAppCredentials(opts.AppId, opts.AppPassword);
            var botId =opts.BotId;

            var stateUrl = new Uri(opts.StateUrl);

            var serviceUrl = "https://store.botframework.com";
            MicrosoftAppCredentials.TrustServiceUrl(stateUrl.AbsoluteUri);
            string continuationToken = "";

            var client = new StateClient(stateUrl, credentials);
            var state = client.BotState;
            BotStateDataResult stateResult = null;
            foreach (var channelId in KnownChannelsIds)
            {
                Console.WriteLine($"***{channelId}***");
                continuationToken = "";
                do
                {
                    try
                    {                
                        // should work with "directline", "facebook", or "kik"
                        stateResult = await BotStateExtensions.ExportBotStateDataAsync(state, channelId, continuationToken).ConfigureAwait(false);
                        foreach (var datum in stateResult.BotStateData)
                        {
                            if ((DateTime.UtcNow - datum.LastModified).HasValue && (DateTime.UtcNow - datum.LastModified).Value.Days < 1)
                            {
                                Console.WriteLine($"LastModified: {datum.LastModified}, UsserId: {datum.UserId}");
                            }
                            if (datum.Data != "{}")
                            {
                                Console.WriteLine($"conversationID: {datum.ConversationId}\tuserId: {datum.UserId}\tdata:{datum.Data}\n");
                                if (targetStore != null)
                                {
                                    var cancellationToken = new CancellationToken();
                                    var address = new Microsoft.Bot.Builder.Dialogs.Address(botId, channelId, datum.UserId, datum.ConversationId, serviceUrl);
                                    var botStoreType = string.IsNullOrEmpty(datum.ConversationId) ? BotStoreType.BotUserData : BotStoreType.BotPrivateConversationData;
                                    var botData = new BotData
                                    {
                                        Data = datum.Data
                                    };
                                    await targetStore.SaveAsync(address, botStoreType, botData, cancellationToken);
                                }
                                else if (outputFile != null)
                                {
                                    var id = new StringBuilder();
                                    id.Append($"{channelId}:");
                                    if (!string.IsNullOrEmpty(datum.ConversationId))
                                    {
                                        id.Append($"private{datum.ConversationId}:");
                                    }
                                    id.Append(datum.UserId);
                                    var serializedData = JsonConvert.SerializeObject(datum.Data);
                                    var outputValue = $"\t{{\r\n\t\"id\": \"{id.ToString()}\",\r\n" + 
                                        $"\t\t\"botId\": \"{botId}\",\r\n" + 
                                        $"\t\t\"channelId\": \"{channelId}\",\r\n" + 
                                        $"\t\t\"conversationId\": \"{datum.ConversationId}\",\r\n" + 
                                        $"\t\t\"userId\": \"{datum.UserId}\",\r\n" + 
                                        $"\t\t\"data\": {serializedData}\r\n\t}},\r\n";
                                    outputFile.Write(outputValue);
                                }
                            }
                        }
                        continuationToken = stateResult.ContinuationToken;
                    }
                    catch (Exception e)
                    {
                        var errorException = e as ErrorResponseException;
                        if (errorException?.Body?.Error?.Message?.ToLower() == "channel not configured for bot")
                        {
                            continue;
                        }
                        Console.WriteLine(e);
                    }

                } while (!string.IsNullOrEmpty(continuationToken));

            }
            Console.Write("Press Enter key to continue:");
            Console.Read();
            if (outputFile != null)
            {
                outputFile.WriteLine("}\r\n");
                outputFile.Flush();
                outputFile.Close();
            }
            // TODO: Do I need to flush the targetStore?  If so, what Address should I use?
            return true;
        }
       

    }
}
