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
            services.AddBot<ContainerBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.OnTurnError = async (context, exception) =>
                {
                    await context.TraceActivityAsync("EchoBot Exception", exception);
                    await context.SendActivityAsync("Sorry, it looks like something went wrong!");
                };

                var dataStore = new MemoryStorage();
                var convState = new ConversationState(dataStore);
                var userState = new UserState(dataStore);
                var state = new BotStateSet(convState, userState);
                options.Middleware.Add(new ConversationState(dataStore));
            });

            // Register the meta-bot dialog state property accessor.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var state = options.Middleware.OfType<BotState>().FirstOrDefault();
                return state.CreateProperty<DialogState>("ContainerDialogState");
            });

            // Register the target bot.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var state = options.Middleware.OfType<BotState>().FirstOrDefault();
                return new EchoBot(state);
            });

            // Register the meta-bot dialog.
            services.AddSingleton(sp =>
            {
                var dialogState = sp.GetRequiredService<IStatePropertyAccessor<DialogState>>();
                var echoBot = sp.GetRequiredService<EchoBot>();
                return new ContainerDialogSet(dialogState, echoBot);
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
