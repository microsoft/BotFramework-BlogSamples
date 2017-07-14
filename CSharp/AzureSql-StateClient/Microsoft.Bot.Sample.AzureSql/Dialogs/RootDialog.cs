using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Microsoft.Bot.Sample.AzureSql.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }



        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var privateData = context.PrivateConversationData;
            var privateConversationInfo = IncrementInfoCount(privateData, BotStoreType.BotPrivateConversationData.ToString());
            var conversationData = context.ConversationData;
            var conversationInfo = IncrementInfoCount(conversationData, BotStoreType.BotConversationData.ToString());
            var userData = context.UserData;
            var userInfo = IncrementInfoCount(userData, BotStoreType.BotUserData.ToString());

            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            await context.PostAsync($"You sent {activity.Text} which was {length} characters. \n\nPrivate Conversation message count: {privateConversationInfo.Count}. \n\nConversation message count: {conversationInfo.Count}.\n\nUser message count: {userInfo.Count}.");

            privateData.SetValue(BotStoreType.BotPrivateConversationData.ToString(), privateConversationInfo);
            conversationData.SetValue(BotStoreType.BotConversationData.ToString(), conversationInfo);
            userData.SetValue(BotStoreType.BotUserData.ToString(), userInfo);

            context.Wait(MessageReceivedAsync);
        }

        public class BotDataInfo
        {
            public int Count { get; set; }
        }

        private BotDataInfo IncrementInfoCount(IBotDataBag botdata, string key)
        {
            BotDataInfo info = null;
            if (botdata.ContainsKey(key))
            {
                info = botdata.GetValue<BotDataInfo>(key);
                info.Count++;
            }
            else
                info = new BotDataInfo() { Count = 1 };

            return info;
        }
    }
}