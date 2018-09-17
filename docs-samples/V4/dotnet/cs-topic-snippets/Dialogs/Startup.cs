using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MetaBot;
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

namespace Dialogs
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
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
            Debug.WriteLine(">>> Running ConfigureServices...");

            services.AddBot<TopicSelectionBot>(options =>
            {
                Debug.WriteLine($">>> Registering a transient {nameof(TopicSelectionBot)} bot and a singleton BotFrameworkAdapter.");

                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.OnTurnError = async (context, exception) =>
                {
                    await context.TraceActivityAsync("EchoBot Exception", exception);
                    await context.SendActivityAsync("Sorry, it looks like something went wrong!");
                };

                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, anything stored in memory will be gone. 
                IStorage dataStore = new MemoryStorage();
                options.State.Add(new ConversationState(dataStore));

                Debug.WriteLine($">>> options.Middleware.Count {options.Middleware.Count}.");
            });

            // Create and register metabot-specific state accessors.
            services.AddSingleton(sp =>
            {
                Debug.WriteLine($">>> Create a singleton state property accessor for the meta-bot.");

                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var state = options.State.OfType<BotState>().FirstOrDefault();

                // Set the MetaBot's state property accessors.
                return state.CreateProperty<DialogState>("metabotDialogState");
            });

            // Create and register state accessors.
            services.AddSingleton(sp =>
            {
                Debug.WriteLine($">>> Create a singleton {nameof(OutterStateAccessors)}.");

                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var state = options.State.OfType<BotState>().FirstOrDefault();

                return new OutterStateAccessors
                {
                    // Set state property accessors for outer bots.
                    DialogState = state.CreateProperty<DialogState>(OutterStateAccessors.DialogStateName),
                    PropertyAccessor = state.CreateProperty<EchoState>(OutterStateAccessors.OuterBotState),
                };
            });

            // Create and register the metabot's selection dialog.
            // This entails creating all of the dialogs for the outter bots.
            services.AddSingleton(sp =>
            {
                Debug.WriteLine($">>> Create a singleton {nameof(SelectionDialogSet)}.");

                var metaStateAccessor = sp.GetRequiredService<IStatePropertyAccessor<DialogState>>();
                var outterStateAccessors = sp.GetRequiredService<OutterStateAccessors>();

                return new SelectionDialogSet(metaStateAccessor, new List<TopicDescriptor>
                {
                    new TopicDescriptor
                    {
                        Name = "Manage simple conversation flow with dialogs",
                        File = "bot-builder-dialog-manage-conversation-flow.md",
                        Sections = new Dictionary<string, IBot>
                        {
                            ["Single step dialog"] = new SimpleConversationFlows.SingleStepDialog(outterStateAccessors.DialogState),
                        },
                    },
                });
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
