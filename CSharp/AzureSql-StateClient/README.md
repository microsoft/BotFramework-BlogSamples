# Explanation

A example State Client using Sql Server for storage.

# Setup

1) Register a bot with the Bot Framework at http://dev.botframework.com/ and add the AppId and AppPassword to Microsoft.Bot.Sample.AzureSql\Web.config.

2) Retrieve a WebChat secret from https://dev.botframework.com/bots/channels?id=[YourBotId]&channelId=webchat and add it to Microsoft.Bot.Sample.AzureSql\default.htm in place of [YourWebChatSecret].

3) Create an Sql Server database and add the connection string to Microsoft.Bot.Sample.AzureSql\Web.config, overwriting the current [BotDataContextConnectionString].

4) Execute 'update-database' from the Nuget Package Manager Console.  This will create the SqlBotDataEntities table.

5) Publish the Bot as an Azure App Service, and add the public endpoint (https://[YourHostUrl]/api/messages) to the Bot Framework portal settings page. Make sure to use https instead of http in the url.
