# Enable speech in the Web Chat control 

  This is a sample HTML file which shows how to enable speech recognition and synthesis in the Web Chat control.

## Prerequisites

  Before you run the sample, you need to have a Direct Line secret or token for the bot that you want to run using the Web Chat control. 
  * See [Connect a bot to Direct Line](https://docs.microsoft.com/en-us/bot-framework/channel-connect-directline) for information on getting a Direct Line secret.
  * See [Generate a Direct Line token](https://docs.microsoft.com/en-us/bot-framework/rest-api/bot-framework-rest-direct-line-3-0-authentication) for information on exchanging the secret for a token.


## Running the sample

To run the sample, you need to choose an option in the sample code, start a web server locally, and then open `index.html` in a browser with the appropriate parameters.

### Choose a speech recognition option

You have a few options to choose from for speech recognition. In `index.html`, find and uncomment the definition of `speechOptions` in the code that corresponds to the option you want to choose. 
  
  | Option | Description |
  |-----------|-------------|
  | No speech | The Web Chat control can be used with speech disabled. |
  | Native browser speech| Browser-provided components handle speech synthesis and generation. Not all browsers support this option. |
  | Cognitive Services speech recognition using an API key | You provide an API key to make calls to the speech recognition and speech synthesis services. This option has cross-browser support.|
  | Cognitive Services speech recognition using a token| Generate a token to avoid exposing your API key. |
  | Custom ISpeechRecognizer and ISpeechSynthesizer| Use this option if you're creating your own speech recognition and synthesis components. |

### Start a web server
One way to start a web server is to use `npm http-server` at a Node.js command prompt.
  
1. To install `http-server` globally so it can be run from the command line, run this command:

```
npm install http-server- -g
```

2. To start a web server using port 8000, from the directory that contains this file, run this command:

```
http-server -p 8000
```

### Open the file in a browser

Open `index.html` by pointing your browser at `http://localhost:8000/index.html?queryparameters`. For example, http://localhost:8000/index.html?s=YOURDIRECTLINESECRET invokes the WebChat control using the DirectLine secret. The parameters that you can set in the query string are described in the following table:

  | Parameter | Description |
  |-----------|-------------|
  | s | Direct Line secret. Either a Direct Line secret or a Direct Line token must be provided. See [Connect a bot to Direct Line](https://docs.microsoft.com/en-us/bot-framework/channel-connect-directline) for information on getting a Direct Line secret. |
  | t | Direct Line token. See [Generate a Direct Line token](https://docs.microsoft.com/en-us/bot-framework/rest-api/bot-framework-rest-direct-line-3-0-authentication) for info on how to generate this token. |
  | domain | Optional. The URL of an alternate Direct Line endpoint.  |
  | webSocket | Optional. Set to 'true' to use WebSocket to receive messages. Default is `false`. |
  | userid | Optional. The ID of the bot user.  |
  | username | Optional. The user name of the bot's user.  |
  | botid | Optional. ID of the bot. |
  | botname | Optional. Name of the bot. |

### Use speech to interact with your bot
Web Chat should display in the browser window. Users interact with the voice interface by using the microphone in the Web Chat control.

![Web chat speech screenshot](./webchat-sample-speech.png)

If the user types instead of speaking a response, Web Chat turns off the speech functionality and the bot gives only a textual response instead of speaking out loud. To re-enable the spoken response, the user can use the microphone to respond to the bot the next time. If the microphone is accepting input, it appears dark or filled-in. If it's grayed out, the user clicks on it to enable it.
> **NOTE:** 
> Google Chrome supports the browser speech recognizer. However, Chrome may block the microphone in the following cases:
> * If the URL of the page that contains Web Chat begins with `http://` instead of `https://`.
> * If the URL is a local file using the `file://` protocol instead of `http://localhost:8000`.

## Additional resources

* For more information on how to use the Web Chat control with speech recognition, see [Enable speech in the Web Chat channel](https://docs.microsoft.com/en-us/bot-framework/channel-connect-webchat-speech).
* You can [download the source code](https://github.com/Microsoft/BotFramework-WebChat) for the web chat control on GitHub.
* The [Bing Speech API documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/speech/home) provides more information on the Bing Speech API.

