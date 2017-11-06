# Stack Overflow Bot

This Bot is intended to demonstrate a number of integrations between Microsoft Bot Framework, and Microsoft's Cognitive Services. From Microsoft's Cognitive Services, this bot uses: Bing Custom Search, Language Understanding Intelligence Service (LUIS), QnA Maker, and Text Analytics. The full range of cognitive services that Microsoft offers can be viewed here: https://azure.microsoft.com/en-us/services/cognitive-services/.

Bot Framework allows you to build bots using a simple, but comprehensive API. The Bots you build can be configured to work for your users wherever they are, whether it's Skype, Microsoft Teams, Cortana, Slack or more. To learn more about Bot Framework, visit https://dev.botframework.com/. To learn more about developing bots with the Bot Framework, visit https://docs.microsoft.com/en-us/bot-framework/#pivot=main&panel=developing.

## Components

### [StackBot](#bot)
A JavaScript project that demonstrates the usage of Bing Custom Search, LUIS, QnA Maker, and Text Analytics to make it easier to search for solutions on Stack Overflow.
### [DialogAnalyzerFunc](#dialog-analyzer-azure-function)
An C# project that uses Computer Vision and Text Analytics to parse the contents of a screenshot.
### [StackCode](#stack-overflow-bot-visual-studio-code-extension)
A TypeScript project that demonstrates how the bot can be built into Visual Studio Code.

## Bot

### Requirements to run the Bot

- Node 8.1.4

### Installation of the Bot

Run `npm install` in the `StackBot` directory.

### Running the Bot

1. Run `npm run start` in the `StackBot` directory.
2. Navigate to `http://localhost:3978/` to start interacting with your bot.

### Companion Visual Studio Code Extension

The Visual Studio Code extension allows developers to use their Bot as a programming buddy, allowing them to amplify their productivity by reducing context switching. For more information about it, including how it can be built, and installed, please refer to the the [Stack Overflow Bot Visual Studio Code Extension](#stack-overflow-bot-visual-studio-code-extension) section below.

### Configuration

As this bot uses a number of services, you will need to create applications and generate keys for each one. The following instructions are intended to guide you on how to do this.

#### Service: Bot Framework

To register a Bot Framework Bot, go to https://dev.botframework.com/bots/new. Fill out the form, select `Register an existing bot using BotBuilder SDK`, and create a `Microsoft App Id` and `Password`. Save both `Microsoft App Id` and `Password` somewhere secure, and add them as the following environment variables. See https://www.schrodinger.com/kb/1842 for instructions on how to set environment variables.

- `BOTBUILDER_APP_ID` is the `Microsoft App Id` you generated
- `BOTBUILDER_APP_PASSWORD` is the `Microsoft App Password` you generated.

#### Service: Language Understanding Intelligence Service (LUIS)

To register and train a LUIS Application, go to https://www.luis.ai/applications. After logging in, click `New App` and follow the directions. When directed to the dashboard, go to Settings, and click `Import Version`. From the Uplaod File dialog,select the `luis.json` file under the `StackBot/data` directory. After the file is uploaded, click the `Set as active version` button under the `Actions` column. Click on `Train & Test` on the Left Column and click `Train Application`. It may take a few minutes to finish. Once done, you can test your LUIS model by typing out an utterance. Try 'tell me a joke', for example. Finally, click on `Publish App`, assign a key, and click `Publish`. You will be
given an `Endpoint url`. Copy this URL and add it as an environment variable.

- `LUIS_MODEL` is the `Endpoint url` from publishing a LUIS Application.

#### Service: QnAMaker

To register a QnAMaker Application, go to https://qnamaker.ai/Create. After logging in, name your new Application and click on `Select file…` under the `FAQ FILES` heading. From the File Picker, navigate to the `smalltalk.tsv` file under the `StackBot/data` directory. Then click `Create`, and wait for a moment as the QnaMaker service is created from the Question/Answer pairs. When prompted, click `Publish` to expose the service to the outside world. You will be directed to a `Success` page. Here, take a look at the `Sample HTTP Request`. It will look like the following, but with generated keys in place of `KB_ID` and `QNA_KEY`, and a URL in place of `QNA_URL`:

```
    POST /knowledgebases/KB_ID/generateAnswer
    Host: QNA_URL
    Ocp-Apim-Subscription-Key: QNA_KEY
    Content-Type: application/json
    {"question":"hi"}
```

Take note of `KB_ID`, `QNA_KEY`, and `QNA_URL`, and save them. Set the following as environment variables:

- `KB_ID` is the `KB_ID` from the sample http request
- `QNA_KEY` is the `QNA_KEY` from the sample http request
- `QNA_URL` is the `QNA_URL` fom the 'Host' parameter. As Cognitive Services can vary from region to region, it's important to set this appropriately. You may see for example, 'https://westus.api.cognitive.microsoft.com/qnamaker/v2.0' if your Text Analytics Cognitive Service is deployed in the `West US` Azure region.

#### Service: Dialog Analyzer

To deploy and configure the Dialog Analyzer Azure Function, please refer to the [Dialog Analyzer Azure Function](#dialog-analyzer-azure-function) section below. After the deployment and configuring the Azure Function, set the following environment variables:

- `DIALOG_ANALYZER_CLIENTID` is the name of the `Function Key`
- `DIALOG_ANALYZER_KEY` is the value of the `Function Key`
- `DIALOG_ANALYZER_URL` is the url of the deployment of the Azure Function

#### Service: Bing Custom Search

To create a new Bing Custom Search, go to https://customsearch.ai/applications. After logging in, click on the `New custom search` with a new custom search instance name. You can then add a new domain `stackoverflow.com` in this case to set your custom search to stackoverflow.com only. Then click on the `Custom Search Endpoint` button, that is located besides your custom search name to obtain the `Primary key` and the `Custom Configuration ID`. For more information, please refer to https://docs.microsoft.com/en-us/azure/cognitive-services/bing-custom-search/quickstart. Set the following environment variables:

- `BING_SEARCH_CONFIG` is the Custom Configuration ID from your custom search instance
- `BING_SEARCH_KEY` is the Primary Key (or Secondary Key) from your custom search instance

#### Service: Text Analytics

To register a Text Analytics Cognitive Service for Sentiment Analysis, go to https://ms.portal.azure.com/#create/Microsoft.CognitiveServices/apitype/TextAnalytics. After logging and going through the process of creating a Text Analytics Cognitive Service, go to the Application dashboard in the Azure Portal. Here you can click on `Keys` in the left pane or `Manage Keys` in the `Essentials` panel. Copy one of the keys shown and save it. Set the following environment variables:

- `TEXT_ANALYTICS_KEY` is one of the keys shown in the Azure Portal.
- `TEXT_ANALYTICS_URL` is the URL shown under `Endpoint` in the `Essentials` panel. As Cognitive Services can vary from region to region, it's important to set this appropriately. You may see for example, 'https://westus.api.cognitive.microsoft.com/text/analytics/v2.0' if your Text Analytics Cognitive Service is deployed in the `West US` Azure region.

#### Service: Ngrok tunneling

In order for your locally running bot to communicate to other Bot Framework channels, it must make itself known to the Bot Framework dashboard, and the world. To do this, you can use tunneling software, like ngrok. Ngrok can be downloaded here https://ngrok.com/download. After installing and running Ngrok on port 3978 with the following commnad `ngrok http 3978`, you should see:

    Session Status                online
    Version                       2.2.8
    Region                        United States (us)
    Web Interface                 http://127.0.0.1:4040
    Forwarding                    http://b05cf662.ngrok.io -> localhost:3978
    Forwarding                    https://b05cf662.ngrok.io -> localhost:3978

    Connections                   ttl     opn     rt1     rt5     p50     p90
                                  0       0       0.00    0.00    0.00    0.00

Copy the HTTPS forwarding URL (in this case `https://b05cf662.ngrok.io`) to your clipboard, and go to the Bot Framework dashboard at https://dev.botframework.com/bots. Select your bot and click on `Settings` in the the upper right corner. From there, you can scroll down to `Messaging Endpoint` under `Configuration`, and paste in the URL you copied, followed by the API endpoint that the bot listens on (in this bot's case: `/api/messages`). For example, if the HTTPS Forwarding URL was `https://b05cf662.ngrok.io` we would use ` https://b05cf662.ngrok.io/api/messages` as the `Messaging Endpoint`.

When the bot is running ([see the section on running the bot](#running-the-bot)), it should now be testable directly from the Bot Framework dashboard. Click `<- Test` in the upper right corner to slide open a Web Chat control and begin interacting with the bot.

To test your bot from a web chat control hosted at the HTTPS forwarding URL, continue reading to setup and configure a Direct Line channel.

#### Service: Locally hosted Web Chat control.

Go to your Bot's dashboard, available at  https://dev.botframework.com/bots and add a 'Direct Line' channel.
This channel allows the Bot Framework Web Chat control to communicate to your Bot's backend service. From there, click `Add a Site` and give it a name. Click `Show` to the right of the hidden keys under `Secret keys`, and copy it. You can come back to this dashboard to view it again if you lose it. From there, open the file at `StackBot/static/index.html`, and change the assignment of the `BOT_SECRET` variable, pasting the Secret Key that was just copied.

## Dialog Analyzer Azure Function

This Azure Function is intended to demonstrate how to build a function with Azure and Microsoft's Cognitive Services. From Microsoft's Cognitive Services, this function uses Computer Vision's Image Analysis to extract tags and captions; and Optical Character Recognition (OCR) to extract text from the image. It also uses Text Analytics to extract key phrases from text.

Azure Functions allows you to focus on building great apps and not have to worry about provisioning and maintaining servers, especially when your workload grows. Functions provides a fully managed compute platform with high reliability and security. With scale on demand, you get the resources you need—when you need them. To learn more about Azure Functions, visit https://azure.microsoft.com/en-us/services/functions/.

### Requirements to build the function

- Visual Studio 2017 version 15.3 or later
- Azure development workload
- .NET Framework 4.6.1

### Publish the function to Azure

To publish the Azure function, you can open the `DialogAnalyzerFunc` Visual Studio project. In `Solution Explorer`, right-click the project and select `Publish`. Choose `Create New` and then click `Publish`. If you haven't already connected Visual Studio to your Azure account, click `Add an account....`. In the `Create App Service` dialog, use the `Hosting` settings to sepecify your `App Name`, `Subscription`, `Resource Group`, `App Service Plan`, and `Storage account`. Click `Create` to create a function app in Azure with the previously populated settings. For more details, please refer to https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs#publish-to-azure

### Configuration

This function uses a number of services. You will need to configure the function's deployment and generate keys for each of the services. The following instructions are intended to guide you on how to do this.

#### Deployment: Dialog Analyzer Azure Function

In order for the bot to use your deployed function, you will need to configure it's application settings.

To begin, go to the `Azure portal` and sign in to your Azure account. In the search bar at the top of the portal, type the name of your function app and select it from the list. In the `Function Apps` panel, expand the function and select the `Manage` button. Within the Manage window, click on `Add new function key` button and define a key name and value for the function and click on the `Save` button. These will become your `DIALOG_ANALYZER_CLIENTID` and `DIALOG_ANALYZER_KEY` settings for your bot.

For more information, please refer to: https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings.

#### Service: Computer Vision

To register a Computer Vision Cognitive Service for Image Analysis and OCR, go to https://portal.azure.com/#create/Microsoft.CognitiveServices/apitype/ComputerVision. After logging and going through the process of creating a Computer Vision Cognitive Service, go to the Application dashboard in the Azure Portal. Here you can click on `Keys` in the left pane or `Manage Keys` in the `Essentials` panel. Copy one of the keys shown and save it. Set the following in your App Settings of your function:

- `COMPUTERVISION_SUB_KEY` is one of the access keys shown in the Azure Portal.
- `COMPUTERVISION_APP_REGION` is region of the the URL shown under `Endpoint` in the `Essentials` panel. For example, if 'https://westus.api.cognitive.microsoft.com/vision/v1.0' is where your Computer Vision Cognitive Service is deployed, then the app region is `westus`.

#### Service: Text Analytics

To register a Text Analytics Cognitive Service for Sentiment Analysis, go to https://ms.portal.azure.com/#create/Microsoft.CognitiveServices/apitype/TextAnalytics. After logging in and going through the process of creating a Text Analytics Cognitive Service, go to the Application dashboard in the Azure Portal. Here you can click on `Keys` in the left pane or `Manage Keys` in the `Essentials` panel. Copy one of the keys shown and save it. Set the following in your App Settings of your function:

- `TEXTANALYTICS_SUB_KEY` is one of the access keys shown in the Azure Portal.
- `TEXTANALYTICS_APP_REGION` is region of the the URL shown under `Endpoint` in the `Essentials` panel. For example, if 'https://westus.api.cognitive.microsoft.com/text/analytics/v2.0' is where your Text Analytics Cognitive Service is deployed, then the app region is `westus`.

## Stack Overflow Bot Visual Studio Code Extension

This Visual Studio Code Extension is intended to be a companion piece to the Stack Overflow Bot. It allows you to quickly
call up the bot using a simple command.

### Configuring the extension to use your own Bot.

After you've deployed the Stack Overflow Bot and its companion function to your favorite hosting service, and registered a bot
with the Bot Framework portal (see https://dev.botframework.com/bots/new), go to your Bot's dashboard, and add a 'Direct Line' channel.
This channel allows the Bot Framework Web Chat control to communicate to your Bot's backend service. From there, Add a Site and give it a name.
You will be directed to a page that will allow you to generate and copy tokens. Click on 'Show' on one of the tokens, and copy it. You can come back to this dashboard to view it again if you lose it. From there, open up your Visual Studio Code User Settings (`⌘,` or `⊞,`), and create a new field `StackCode.directLineToken`, assigning the token you copied to this label. If done correctly, activating the Bot in Visual Studio Code will open a pane that will show you a Bot Framework WebChat control, where you can interact with the bot.

### Installing dependencies

Run `npm install`

### Installing the extension

A few options…

* Option 1: Clone the package using git, then open it in code. Go to the Debug tab and run it in an extension host window.
* Option 2: Clone the package using git, run `code --install-extension StackCode-0.1.1.vsix` in the directory. See the VS Code [docs](https://code.visualstudio.com/docs/editor/extension-gallery#_install-from-a-vsix) for more details.

### Activating (running) the extension

* Using the command palate (`⇧⌘P` or `⇧⊞P`), type out `Start Stack Overflow Bot`, or something close to it. The bot will appear in its
own pane to the right.

## License

MIT. See LICENSE file.