# Bot Builder SDK Azure Extensions

The Microsoft Bot Builder SDK Azure Extensions allow for interactions with specific Azure components.

## High level features:

* Azure Table Storage: Allows bot developers to store bot state in their own Azure Storage accounts. For more information on Azure Table Storage, visit the **[Azure Table Storage Documentation](https://azure.microsoft.com/en-us/services/storage/tables/)**
* Azure DocumentDB: Allows bot developers to store bot state in DocumentDB. For more information on Azure DocumentDB, visit the **[Azure DocumentDB Documentation](https://azure.microsoft.com/en-us/services/documentdb/)**

## Sample Overview

This is a simple 'echo' bot which echo's the user's message. The difference is that this sample demonstrates
how to leverage the [botbuilder-azure](https://www.npmjs.com/package/botbuilder-azure) npm module to save your bot's conversation state to an Azure database. 

## To run this sample

After you've cloned this repo, cd into the folder and install the dependencies with npm:

  npm install

## Configure the Bot to your Azure DB

```
  var documentDbOptions = {
      host: 'Your-Azure-DocumentDB-URI',
      masterKey: 'Your-Azure-Key',
      database: 'botdocdb',
      collection: 'botdata'
  };
```

## Using the Bot Framework Emulator with this sample

To use the emulator, you'll need to add an environment property.
In .env file, add "NODE_TLS_REJECT_UNAUTHORIZED = 0;" or, alternatively in your bot
you can directly paste the following:

  process.env.NODE_TLS_REJECT_UNAUTHORIZED = 0;

Make sure to delete/comment out the above property if you switch to a production channel.

## Run the bot
Run the following command to start the bot

  node app.js
