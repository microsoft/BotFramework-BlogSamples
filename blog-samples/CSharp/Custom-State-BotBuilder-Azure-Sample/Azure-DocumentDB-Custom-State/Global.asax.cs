using System.Reflection;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Configuration;
using System;

namespace Azure_DocumentDB_Custom_State
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Conversation.UpdateContainer(
                builder =>
                {
                    builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                    // Bot Storage: register state storage for your bot
                    // Default store: volatile in-memory store - Only for prototyping!
                    // var store = new InMemoryDataStore();

                    var uri = new Uri(ConfigurationManager.AppSettings["DocumentDBUri"]);
                    var key = ConfigurationManager.AppSettings["DocumentDBKey"];

                    var store = new DocumentDbBotDataStore(uri, key);

                    builder.Register(c => store)
                        .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                        .AsSelf()
                        .SingleInstance();
                });
                
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
