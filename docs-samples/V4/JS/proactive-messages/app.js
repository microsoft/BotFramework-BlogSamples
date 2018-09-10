/*
 * Botbuilder v4 SDK - Proactive Messages.
 * 
 * This bot demonstrates how to implement proactive messaging. This example uses temporary storage to 
 * save the user's reference. A timer is set to emulate timelapse.
 * Once the timer goes off, the bot reads the user's reference back in from storage then sends the user
 * a message proactively.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * 
 * 2) From VSCode, open the package.json file and make sure that "main" is not set to any path (or is undefined) 
 * 3) Navigate to your bot app.js file and run the bot in debug mode (eg: click Debug/Start debuging)
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

// Required packages for this bot
const { BotFrameworkAdapter, MemoryStorage, BotState, BotStateSet, TurnContext } = require('botbuilder');
const restify = require('restify');

// Create server
let server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`${server.name} listening to ${server.url}`);
});

// Create adapter
const adapter = new BotFrameworkAdapter({ 
    appId: process.env.MICROSOFT_APP_ID, 
    appPassword: process.env.MICROSOFT_APP_PASSWORD 
});

// Storage
const storage = new MemoryStorage(); // For production, use permanent storage such as CosmosDb.
const jobState = new BotState(storage, (context) => 'jobState');
const jobLogAccessor = jobState.createProperty('jobLog');
adapter.use(new BotStateSet(jobState));

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    // Route received activity to adapter for processing
    adapter.processActivity(req, res, async (context) => {
        var isMessage = context.activity.type === 'message';
        if (isMessage) {
            const jobs = await jobLogAccessor.get(context, new jobLog()); // Create a new jobLog if it doesn't exists
            const utterance = (context.activity.text || '').trim().toLowerCase();

            // If user types in run, create a new job
            if(utterance.match(/run|run job/gi)){
                var jobData = await createJob(jobs, context);
                await context.sendActivity(`We're starting job ${jobData.timeStamp} for you. We'll notify you when it's complete.`);
            }
            else if(utterance.match(/show|show jobs/gi)){
                // Display all job information in the log
                if(jobs.count > 0){
                    var msg = "| Job number &nbsp; | Conversation ID &nbsp; | Completed |<br>" +
                    "| :--- | :---: | :---: |<br/>";
                    for(j in jobs.jobList){
                        let jobInfo = jobs.jobList[j];
                        msg += `| ${jobInfo.timeStamp} | ${jobInfo.conversationReference.conversation.id} | ${jobInfo.completed} | <br/>`;
                    }
                    await context.sendActivity(msg);
                }
                else{
                    await context.sendActivity("The job log is empty.");
                }
            }
            
            // Check whether this is simulating a job completed event.
            var [done, jobId, ...tail] = utterance.split(' ');
            if(done.match(/done/i)){
                // If the user types done and a Job Id Number
                // We check if jobId is a number
                if (!isNaN(parseInt(jobId))) {
                    var jobInfo = jobs.jobList[jobId]; // Get the job
                    if(!jobInfo){
                        await context.sendActivity(`The log does not contain job number ${jobId}.`);
                    }
                    else if(jobInfo.completed){
                        await context.sendActivity(`Job number ${jobId} is already completed.`)
                    }
                    else{
                        await context.sendActivity(`Completing job ${jobId}...`);
                        await completeJob(adapter, context, jobInfo);
                    }
                    
                } else if (isNaN(parseInt(jobId))) {
                    await context.sendActivity('Enter "done" followed by a job ID number.');
                };
            }
        } 

        // TODO: POST-IGNITE
        // Handle event activity
        // On a job completed event, mark the job as complete and notify the user.
        else if (context.activity.type === 'event' && context.activity.name === 'jobCompleted') {
            var jobId = context.activity.value;
            if (!isNaN(parseInt(jobId))) {
                var jobInfo = jobs.jobList[jobId];
                if(jobInfo && !jobInfo.completed){ 
                    await completeJob(adapter, context, jobInfo);
                }
            }
        }
        
        // Send default message
        if(!context.responded && isMessage) {
            await context.sendActivity(
                "Type `run` or `run job` to start a new job.<br>" +
                "Type `show` or `show jobs` to display the job log.<br>" +
                "Type `done <jobNumber>` to complete a job."
            )
        }
    });
});


// Helper function to check if object is empty
function isEmpty(obj) {
    for(var key in obj) {
        if(obj.hasOwnProperty(key))
            return false;
    }
    return true;
};

// Define a data structure for the job log
// jobList is an associated object where: jobList[jobID] = jobData
// jobData is initialized in the createJob method.
function jobLog() {
    this.count = 0;     // Track number of jobs in the listing
    this.jobList = {};  // List of jobs
}

// Create and start a new job
async function createJob(jobs, context){
    // Define a new job
    var jobData = {
        "timeStamp": Date.now(), // Used as jobId
        "completed": false,
        "conversationReference": await TurnContext.getConversationReference(context.activity)
    }

    jobs.jobList[jobData.timeStamp] = jobData; // Add the job
    jobs.count++;   // Increment counter
    return jobData; // Return the data
}

// Complete a job
async function completeJob(adapter, context, jobInfo){
    await adapter.continueConversation(jobInfo.conversationReference, createCallback(jobInfo));
    await context.sendActivity(`Job completed.`); // Message sent to current conversation.
}

// Helper function to handle callback from continueConversation method.
function createCallback(jobInfo){
    return async (context) => {
        var jobs = await jobLogAccessor.get(context, new jobLog()); // Get the job log from state
        jobs.jobList[jobInfo.timeStamp].completed = true;           // Book keeping
        // Proactive message is sent to the jobInfo's referenced conversation.
        await context.sendActivity(`Job ${jobInfo.timeStamp} is complete.`);
    }
}
