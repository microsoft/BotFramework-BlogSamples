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
using ContainerLib;
using System.Collections.Generic;

namespace TesterBot
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
            // Register the meta-bot, along with its state.
            services.AddBot<TopicSelectorBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.OnTurnError = async (context, exception) =>
                {
                    await context.TraceActivityAsync("EchoBot Exception", exception);
                    await context.SendActivityAsync("Sorry, it looks like something went wrong!");
                };

                var dataStore = new MemoryStorage();
                options.State.Add(new ConversationState(dataStore));
                options.State.Add(new MetaState(dataStore, "meta"));
            });

            // Register the meta-bot dialog state property accessor.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var metaState = options.State.OfType<MetaState>().First();
                return metaState.CreateProperty<DialogState>("Meta.DialogState");
            });

            // Register the meta-bot dialog.
            services.AddSingleton(sp =>
            {
                var dialogState = sp.GetRequiredService<IStatePropertyAccessor<DialogState>>();
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var convState = options.State.OfType<ConversationState>().First();
                var topics = new List<TopicDescriptor>
                {
                    new TopicDescriptor
                    {
                        Name = "EchoBot",
                        File = "None",
                        Sections = new Dictionary<string, Type>
                        {
                            ["First"] = typeof(EchoBot),
                        },
                    }
                };
                return new TopicSelectorDialogSet(dialogState, topics, convState);
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
