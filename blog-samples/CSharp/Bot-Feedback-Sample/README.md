# QnA Bot sample - Suggested Actions to handle user feedback, with Azure Application Insights

This bot sample using the .NET SDK is a continuation of the ['QnA Rich Cards'](https://github.com/Microsoft/BotFramework-Samples/tree/master/blog-samples/CSharp/Qna-Rich-Cards) sample within this repo. This bot sample was developed to accompany the Bot Framework blog post - [QnA revisited, with Suggested Actions and App Insights](https://blog.botframework.com/2017/09/28/qna-maker-revisited-suggested-actions-app-insights/)

This sample aims to:

1. Demonstrate ease and flexibility of [Suggested Actions](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-add-suggested-actions) within the Bot Builder SDK, using Suggested Actions to implement a user 'feedback' thumbs up/down feature. 

![Feedback with Suggested Actions][pic1]

2. Demonstrate how to add [Application Insights](https://azure.microsoft.com/en-us/services/application-insights/) to a bot project, and track custom events. 

![App Insights Metrics][pic2]

> **Note**: The application insights configuration for this sample is for demonstration only. Application Insights will not work on cloned/forked copies of this repo, and will need to be added to your own project. Don't worry, it's easy - just [click here](https://docs.microsoft.com/en-us/azure/application-insights/app-insights-asp-net) to read about how. 

## Prerequisites
- [Visual Studio 2015 or 2017 Community](https://www.visualstudio.com/downloads/)
- [Bot Application Template](http://aka.ms/bf-bc-vstemplate)
- [BotBuilder-CognitiveServices](https://www.nuget.org/packages/Microsoft.Bot.Builder.CognitiveServices/) NuGet package 
- [Bot Framework Emulator](https://docs.microsoft.com/en-us/bot-framework/debug-bots-emulator)

[pic1]: ../../images/suggested-actions-feedback.png
[pic2]: ../../images/app-insights-metrics.png

## Notes:
- You will need to add Application Insights to your own project, [click here](https://docs.microsoft.com/en-us/azure/application-insights/app-insights-asp-net) to read how. 

- You will need to publish your own QnA knowledge base service - [click here](https://qnamaker.ai/Documentation/Quickstart) for the QnA maker overview, or you can review [this blog post](https://blog.botframework.com/2017/08/25/qna-maker-rich-card-attachments-net/) which walks you through how to setup and deploy your own QnA service. 
