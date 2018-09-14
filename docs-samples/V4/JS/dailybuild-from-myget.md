# How to pull lastest JS build from MyGet feed

For those wanting to test their existing bots against the nightly build, you can pull latest from MyGet feed by following the instructions outlined in this article.

This instruction assumes you are starting a new bot from scratch. It also assumes that you have Node already installed on your machine.

## Update node_modules from MyGet feed

1. If you have an existing bot, copy the "package.json" file of your bot and paste the "package.json" file to a new folder. This new folder is your new bot's root directory. Otherwise, create a new "package.json" file and add this JSON object to the file:
    ```json
    {
        "name": "echobot",
        "dependencies": {
            "botbuilder": "^4.0.0-preview1.1",
            "restify": "^4.3.0"
        },
        "devDependencies": {
            "@types/node": "^6.0.52",
            "@types/restify": "^2.0.35"
        }
    }
    ```

2. Run the following command to pull from MyGet: `npm install --registry=https://botbuilder.myget.org/F/botbuilder-v4-js-daily/npm/`

  > [!TIPS]
  > To pull from MyGet again or to add new libraries to the project (by adding the new library to the dependencies object of the "package.json" file), delete the "node_modules" folder and "package-lock.json" file before running the above command. That will force the installer to pull from MyGet feed instead of from a cache. 
  > Other packages you may need in add to the "dependencies" list:
  >     "botbuilder-dialogs": "^4.0.0-preview",
  >     "botbuilder-ai": "^4.0.0-preview1.2",
  > For a complete list of packages available, see https://botbuilder.myget.org/gallery/botbuilder-v4-js-daily

## Verify library versions

To verify that the packages are updated to latest, check the library's version to see if it matches up with the version found on MyGet.

1. Navigate to the library and look at the "package.json" for the version number. For example, to check the Botbuilder library's version, open the file `node_modules\botbuilder\package.json`. See the line: `"_id": "botbuilder@4.0.0-preview1.37334"`.
2. Compare the version number (e.g.: preview1.37334) against the version number found on MyGet site:
https://botbuilder.myget.org/gallery/botbuilder-v4-js-daily. 
