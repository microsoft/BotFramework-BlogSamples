# MockChannel
When creating load tests as described in [Load testing a Bot](https://blog.botframework.com/2017/06/19/Load-Testing-A-Bot/) you need 
a service to pass as the activity.ServiceUrl.

This is a sample implementation of that callback service.

To use:
* build and deploy (say http://yourmockservice.azurewebsites.net)
* When posting an activity to your bot from your webTest, set the activity.ServiceUrl = "http://yourmockservice.azurewebsites.net" 

When you run your load test generating requests to your bot, when the bot posts back it will post back to the ServiceUrl 
in the activity which is this mock service.
