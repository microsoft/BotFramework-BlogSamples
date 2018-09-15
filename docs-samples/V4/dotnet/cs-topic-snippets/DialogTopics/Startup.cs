//#define Addition
//#define Greeting
//#define Hotel
//#define Prompts
#define IntegratedDialogs

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DialogTopics
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
#if Addition
            services.AddBot<AdditionBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.OnTurnError = async (context, exception) =>
                {
                    await context.TraceActivityAsync("Bot Exception", exception);
                    await context.SendActivityAsync("Sorry, it looks like something went wrong!");
                };

                IStorage dataStore = new MemoryStorage();
                options.Middleware.Add(new ConversationState(dataStore));
            });

            // Create and register the dialog state accessor.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var convState = options.Middleware.OfType<ConversationState>().FirstOrDefault();
                return convState.CreateProperty<DialogState>($"DialogSet.DialogStateAccessor");
            });

            // Create and register the addition dialog set.
            services.AddSingleton(sp =>
            {
                var accessor = sp.GetRequiredService <IStatePropertyAccessor<DialogState>>();
                return new AdditionDialogSet(accessor);
            });
#elif Greeting
            services.AddBot<GreetingBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.OnTurnError = async (context, exception) =>
                {
                    await context.TraceActivityAsync("Bot Exception", exception);
                    await context.SendActivityAsync("Sorry, it looks like something went wrong!");
                };

                IStorage dataStore = new MemoryStorage();
                options.Middleware.Add(new ConversationState(dataStore));
            });

            // Create and register the dialog state accessor.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var convState = options.Middleware.OfType<ConversationState>().FirstOrDefault();
                return convState.CreateProperty<DialogState>($"DialogSet.DialogStateAccessor");
            });

            // Create and register the greeting dialog set.
            services.AddSingleton(sp =>
            {
                var accessor = sp.GetRequiredService<IStatePropertyAccessor<DialogState>>();
                return new GreetingDialogSet(accessor);
            });
#elif Hotel
            services.AddBot<HotelDialogBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.OnTurnError = async (context, exception) =>
                {
                    await context.TraceActivityAsync("Bot Exception", exception);
                    await context.SendActivityAsync("Sorry, it looks like something went wrong!");
                };

                IStorage dataStore = new MemoryStorage();
                options.Middleware.Add(new ConversationState(dataStore));
            });

            // Create and register the dialog state accessor.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var convState = options.Middleware.OfType<ConversationState>().FirstOrDefault();
                return convState.CreateProperty<DialogState>($"DialogSet.DialogStateAccessor");
            });

            // Create and register the Hotel dialog set.
            services.AddSingleton(sp =>
            {
                var accessor = sp.GetRequiredService<IStatePropertyAccessor<DialogState>>();
                return new HotelDialogSet(accessor);
            });
#elif Prompts
            services.AddBot<PromptsDialogBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.OnTurnError = async (context, exception) =>
                {
                    await context.TraceActivityAsync("Bot Exception", exception);
                    await context.SendActivityAsync("Sorry, it looks like something went wrong!");
                };

                IStorage dataStore = new MemoryStorage();
                options.Middleware.Add(new ConversationState(dataStore));
            });

            // Create and register the dialog state accessor.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var convState = options.Middleware.OfType<ConversationState>().FirstOrDefault();
                return convState.CreateProperty<DialogState>("DialogSet.DialogStateAccessor");
            });

            // Create and register the prompts dialog set.
            services.AddSingleton(sp =>
            {
                var accessor = sp.GetRequiredService<IStatePropertyAccessor<DialogState>>();
                return new PromptsDialogSet(accessor);
            });
#elif IntegratedDialogs
            services.AddBot<IntegratedDialogs.HotelBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.OnTurnError = async (context, exception) =>
                {
                    await context.TraceActivityAsync("Bot Exception", exception);
                    await context.SendActivityAsync("Sorry, it looks like something went wrong!");
                };

                // We're using both conversation and user state.
                IStorage dataStore = new MemoryStorage();

                var convState = new ConversationState(dataStore);
                options.State.Add(convState);

                var userState = new UserState(dataStore);
                options.State.Add(userState);
            });

            // Create and register the dialog state accessor off of conversation state.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var convState = options.State.OfType<ConversationState>().FirstOrDefault();
                return convState.CreateProperty<DialogState>("IntegratedDialogs.DialogState");
            });

            // Create and register the user profile state accessor off of user state.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var userState = options.State.OfType<UserState>().FirstOrDefault();
                return userState.CreateProperty<IntegratedDialogs.UserProfile>("IntegratedDialogs.UserProfile");
            });

            // Create and register the main dialog set.
            services.AddSingleton(sp =>
            {
                var dialogState = sp.GetRequiredService<IStatePropertyAccessor<DialogState>>();
                var userProfileAccessor = sp.GetRequiredService<IStatePropertyAccessor<IntegratedDialogs.UserProfile>>();
                return new IntegratedDialogs.MainDialog(dialogState, userProfileAccessor);
            });
#endif
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
