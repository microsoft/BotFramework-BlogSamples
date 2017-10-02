# QnA Rich Cards Bot Sample

This bot sample using the .NET SDK is to demonstrate two things:

1. How to connect a bot to a QnA service using the [Bot Builder Cognitive Services](https://www.nuget.org/packages/Microsoft.Bot.Builder.CognitiveServices/) NuGet package, open source on Github [here](https://github.com/Microsoft/BotBuilder-CognitiveServices). 

2. How to implement overrides to the default QnAMakerDialog implementation such that a developer can 'intercept' the response activity from the QnA service and customize the reply to be posted back to a user. In this sample bot, we format the response from the QnA service into [rich card attachments](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-add-rich-card-attachments).

![Qna Rich Card response][pic1]

## QnA Maker overview

[Click here for the QnA Maker portal](https://qnamaker.ai/)

![QnA Portal][pic2]

One of the basic requirements in writing your own Bot service is to seed it with questions and answers. In many cases, the questions and answers already exist in content like FAQ URLs/documents, etc.

Microsoft QnA Maker is a free, easy-to-use, REST API and web-based service that trains AI to respond to user's questions in a more natural, conversational way. Compatible across development platforms, hosting services, and channels, QnA Maker is the only question and answer service with a graphical user interface—meaning you don’t need to be a developer to train, manage, and use it for a wide range of solutions.

With optimized machine learning logic and the ability to integrate industry-leading language processing with ease, QnA Maker distills masses of information into distinct, helpful answers.


## Prerequisites

- [Visual Studio 2015 or 2017 Community](https://www.visualstudio.com/downloads/)
- [Bot Application Template](http://aka.ms/bf-bc-vstemplate)
- [BotBuilder-CognitiveServices](https://www.nuget.org/packages/Microsoft.Bot.Builder.CognitiveServices/) NuGet package 
- [Bot Framework Emulator](https://docs.microsoft.com/en-us/bot-framework/debug-bots-emulator)

[pic1]: ../../images/qna-rich-cards.png
[pic2]: ../../images/qna-portal.png
