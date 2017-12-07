# Azure DocumentDB Sample

This simple echo bots illustrates how to use your own Azure Table Storage to store the bot state.

To use Azure Table Store, we configure the Autofac Dependency Injection in [Global.asax](Global.asax.cs). Particularly the following is the piece of code that configures injection of Azure DocumentDB (CosmosDB) Storage:

>Note: DocumentDB was rebranded as CosmosDB - [see here](https://buildazure.com/2017/05/10/cosmosdb-the-new-documentdb-nosql-database-in-microsoft-azure/) for details

```csharp
var store = new TableBotDataStore(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
builder.Register(c => store)
    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
    .AsSelf()
    .SingleInstance();
```

## References
- Documentation - [State Data for Bots in .NET](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-state) 
- Source code for [BotBuilder-Azure on GitHub](https://github.com/Microsoft/BotBuilder-Azure)
- Nuget packaget for .NET [Microsoft.Bot.Builder.Azure](https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/)
- Get started with [Azure DocumentDB](https://docs.microsoft.com/en-us/azure/cosmos-db/create-documentdb-dotnet)

