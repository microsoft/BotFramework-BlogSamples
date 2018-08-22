using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ReferenceBot
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBot<EchoBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.OnTurnError = async (context, exception) =>
                {
                    await context.TraceActivityAsync("EchoBot Exception", exception);
                    await context.SendActivityAsync("Sorry, it looks like something went wrong!");
                };

                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, anything stored in memory will be gone. 
                IStorage dataStore = new MemoryStorage();
                //options.Middleware.Add(new ConversationState<EchoState>(dataStore));
                options.Middleware.Add(new ConversationState(dataStore));
            });

            // Create and register state accessors.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value
                    ?? throw new InvalidOperationException(
                        "BotFrameworkOptions must be configured prior to setting up the state accessors.");

                var convState = options.Middleware.OfType<ConversationState>().FirstOrDefault()
                    ?? throw new InvalidOperationException(
                        "Conversation state must be defined and added before adding conversation-scoped state accessors.");

                return new StateAccessors
                {
                    EchoStateAccessor = convState.CreateProperty<EchoState>(StateAccessors.EchoStateKey),
                    DialogStateAccessor = convState.CreateProperty<DialogState>(StateAccessors.DialogStateKey),
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
