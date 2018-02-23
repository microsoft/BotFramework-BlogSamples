# MockChannel
When doing load tests as described in [Load testing a Bot](https://blog.botframework.com/2017/06/19/Load-Testing-A-Bot/) you need 
a service to pass as the ServiceUrl paramter.  This service implements the Bot Framework channel API for the bot.

To do the load test, you need a mock service which implements this callback.  This is a sample implementation of such a mock service.

To use:
* build and deploy (say http://yourmockservice.azurewebsites.net)
* When posting an activity to your bot from your webTest, set the activity.ServiceUrl = "http://yourmockservice.azurewebsites.net" 

When you run your load test generating requests to your bot, when the bot posts back it will post back to the ServiceUrl 
in the activity which is this mock service.
