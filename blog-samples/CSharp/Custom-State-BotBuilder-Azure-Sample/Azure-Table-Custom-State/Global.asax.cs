using System.Reflection;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Configuration;

namespace Azure_Table_Custom_State
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

                    // This sample will use Azure Table 
                    var store = new TableBotDataStore(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
          
                    builder.Register(c => store)
                        .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                        .AsSelf()
                        .SingleInstance();                 
                });

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
