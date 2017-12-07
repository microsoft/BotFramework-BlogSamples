# Custom state data for your Bots

The [Bot Framework State Service](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-state) API is not recommended for production environments. Currently, every bot built with the SDK comes with this setting by default, but it is only meant for prototyping. 

These samples are simple echo bots which leverage the [Microsoft.Bot.Builder.Azure](https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/) Nuget package to easily create a custom state data store for your bots. Currently, the package supports seamless integration with [Azure Table storage](https://docs.microsoft.com/en-us/azure/cosmos-db/table-storage-how-to-use-dotnet) and [Azure DocumentDB](https://docs.microsoft.com/en-us/azure/cosmos-db/create-documentdb-dotnet). 

Creating a custom state store for your bot provides several benefits:
- Improved latency for your Bot
- Direct control over your bot's state data, which includes information about your users, conversation state, and conversation context. 

