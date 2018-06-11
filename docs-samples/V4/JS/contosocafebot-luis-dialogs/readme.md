# contosocafebot-luis-dialogs 

This sample bot is integrated with a LUIS app that recognizes intents and entities for making a restaurant reservation.

## Intents
Intents represent what the user wants to do. The bot can start a dialog or conversation flow based on the intent recognized by LUIS. This bot recognizes three intents.

|Intent| Example |
|-----|-----|
|Book table | reserve table for 4 at 6:15pm 5/24/2018 <br> reserve a table|
|Greeting| hi <br/> Hello|
|Who_are_you| who are you |

## Entities 

Besides recognizing intent, a LUIS can also extract entities, which are important words for fulfilling a user's request. For example, in the example of a restaurant reservation, the LUIS app might be able to extract the party size, reservation date or restaurant location from the user's message. 

|Entity type| Example |
|-----|-----|
|`PartySize` | reserve table for `4` at 6:15pm 5/24/2018 |
|`datetime`| reserve table for 4 at `6:15pm 5/24/2018`|

## Create the LUIS app

1. Log in to https://www.luis.ai. 
2. In the **My apps** tab, click on the **Import new app** button and choose the JSON file **cafeLUISModel.json** for the app to import.
3. [Train](https://docs.microsoft.com/en-us/azure/cognitive-services/LUIS/luis-how-to-train) the new app.
4. [Publish](https://docs.microsoft.com/en-us/azure/cognitive-services/LUIS/publishapp) the new app.

## Copy the LUIS Subscription Key to use in the bot's code

Copy the App ID from **Settings**, and copy the [LUIS Authoring Key](https://docs.microsoft.com/en-us/azure/cognitive-services/LUIS/luis-concept-keys#authoring-key). Paste them into the bot code in luisbot.ts.

```ts
// Replace this appId with the ID of the app you create from cafeLUISModel.json
const appId = "YOUR-LUIS-APP-ID"
// Replace this with your authoring key
const subscriptionKey = "YOUR-LUIS-SUBSCRIPTION-KEY"
```

## Use LUISGen to generate types for the LUIS results
You can use the [LUISGen tool](https://github.com/Microsoft/botbuilder-tools/tree/master/LUISGen) to generate types that make it easier to work with LUIS results in your bot's code. The tool takes a JSON file for an exported LUIS app as input.

At a Node.js command line, install `luisgen` to the global path.
```
npm install -g luisgen
```

In the root folder of this sample, this LUISGen command was used to generate **CafeLUISModel.ts**:

```
luisgen cafeLUISModel.json -ts CafeLUISModel
```

## Building the bot
You'll need the latest TypeScript compiler installed:

```
npm install --global typescript
```

To compile the sample, run `tsc` from the root directory.

Install dependencies before you run the bot, by running `npm install` in the root directory of the sample:

```
npm install
```

## Using typed LUIS results

You can get a `CafeLUISModel` result from the LUIS recognizer in bot code like this:

```typescript
// call LUIS and get typed results
await luisRec.recognize(context).then(async (res : any) => 
{    
    // get a typed result
    var typedresult = res as CafeLUISModel;  

}    
```

## Pass the LUIS result to a dialog

Examine the code in **luisbot.ts**. In the `processActivity` handler, the bot passes the typed result to the `reserveTable` dialog.

```typescript
// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    // Route received request to adapter for processing
    adapter.processActivity(req, res, async (context) => {
        const isMessage = context.activity.type === 'message';

        // Create dialog context 
        const state = conversationState.get(context);
        const dc = dialogs.createContext(context, state);
            
        if (!isMessage) {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }

        // Check to see if anyone replied. 
        if (!context.responded) {
            await dc.continue();
            // if the dialog didn't send a response
            if (!context.responded && isMessage) {

                
                await luisRec.recognize(context).then(async (res : any) => 
                {    
                    var typedresult = res as CafeLUISModel;                
                    let topIntent = LuisRecognizer.topIntent(res);    
                    switch (topIntent)
                    {
                        case Intents.Book_Table: {                        
                            await dc.begin('reserveTable', typedresult);
                            break;
                        }
                        
                        case Intents.Greeting: {
                            await context.sendActivity("Hi!");
                            break;
                        }
    
                        case Intents.Who_are_you_intent: {
                            await context.sendActivity("I'm the Contoso Cafe Bot.");
                            break;
                        }
                        default: {
                            await dc.begin('default', topIntent);
                            break;
                        }
                    }
    
                }, (err) => {
                    // there was some error
                    console.log(err);
                }
                );                                
            }
        }
    });
});
```


## Check for existing entities in a dialog

In **luisbot.ts**, the `reserveTable` dialog calls a `SaveEntities` helper function to check for entities detected by the LUIS app. If the entities are found, they're saved to dialog state. Each waterfall step in the dialog checks if an entity was saved to dialog state, and if not, prompts for it.

```typescript
dialogs.add('reserveTable', [
    async function(dc, args, next){
        var typedresult = args as CafeLUISModel;

        // Call a helper function to save the entities in the LUIS result
        // to dialog state
        await SaveEntities(dc, typedresult);

        await dc.context.sendActivity("Welcome to the reservation service.");
        
        if (dc.activeDialog.state.dateTime) {
            await next();     
        }
        else {
            await dc.prompt('dateTimePrompt', "Please provide a reservation date and time.");
        }
    },
    async function(dc, result, next){
        if (!dc.activeDialog.state.dateTime) {
            // Save the dateTimePrompt result to dialog state
            dc.activeDialog.state.dateTime = result[0].value;
        }

        // If we don't have party size, ask for it next
        if (!dc.activeDialog.state.partySize) {
            await dc.prompt('textPrompt', "How many people are in your party?");
        } else {
            await next();
        }
    },
    async function(dc, result, next){
        if (!dc.activeDialog.state.partySize) {
            dc.activeDialog.state.partySize = result;
        }
        // Ask for the reservation name next
        await dc.prompt('textPrompt', "Whose name will this be under?");
    },
    async function(dc, result){
        dc.activeDialog.state.Name = result;

        // Save data to conversation state
        var state = conversationState.get(dc.context);

        // Copy the dialog state to the conversation state
        state = dc.activeDialog.state;

        // TODO: Add in <br/>Location: ${state.cafeLocation}
        var msg = `Reservation confirmed. Reservation details:             
            <br/>Date/Time: ${state.dateTime} 
            <br/>Party size: ${state.partySize} 
            <br/>Reservation name: ${state.Name}`;
            
        await dc.context.sendActivity(msg);
        await dc.end();
    }
]);
```

The `SaveEntities` helper function checks for `datetime` and `partysize` entities. The `datetime` entity is a [prebuilt entity](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-reference-prebuilt-entities#builtindatetimev2).

```typescript
// Helper function that saves any entities found in the LUIS result
// to the dialog state
async function SaveEntities( dc: DialogContext<TurnContext>, typedresult) {
    // Resolve entities returned from LUIS, and save these to state
    if (typedresult.entities)
    {
        let datetime = typedresult.entities.datetime;
        if (datetime) {
            // Use the first date or time found in the utterance
            var timexValue;
            if (datetime[0].timex) {
                timexValue = datetime[0].timex[0];
                // More information on timex can be found here: 
                // http://www.timeml.org/publications/timeMLdocs/timeml_1.2.1.html#timex3                                
                // More information on the library which does the recognition can be found here: 
                // https://github.com/Microsoft/Recognizers-Text

                if (datetime[0].type === "datetime") {
                    // in this sample, a datetime detected by LUIS is saved in timex format.
                    dc.activeDialog.state.dateTime = timexValue;
                    // If you want to additionally parse timex, 
                    // use @microsoft/recognizers-text-data-types-timex-expression 
                } 
                else  {
                    console.log(`Type ${datetime[0].type} is not yet supported. Provide both the date and the time.`);
                }
            }                                                
        }
        let partysize = typedresult.entities.partySize;
        if (partysize) {
            console.log(`partysize entity defined.${partysize}`);
            // use first partySize entity that was found in utterance
            dc.activeDialog.state.partySize = partysize[0];
        }
        let cafelocation = typedresult.entities.cafeLocation;

        if (cafelocation) {
            console.log(`location entity defined.${cafelocation}`);
            // use first cafeLocation entity that was found in utterance
            dc.activeDialog.state.cafeLocation = cafelocation[0][0];
        }
    } 
}
```

### Entity types
The code in `SaveEntities` checked `CafeLUISModel` type's `entities` property, which was defined in **CafeLUISModel.ts**.

```typescript
export interface CafeLUISModel {
    text: string;
    alteredText?: string;
    intents: _Intents;
    entities: _Entities;
    [propName: string]: any;
}
```

The `entities` property includes both simple entities, built-in entities, and lists.

```js
export interface _Entities {
    // Simple entities
    partySize?: string[];

    // Built-in entities
    datetime?: DateTimeSpec[];
    number?: number[];

    // Lists
    cafeLocation?: string[][];
    $instance : _Instance;
}
```

The `datetime` type is an array of `DateTimeSpec`:

```js
// datetime is an array of type 
DateTimeSpec {
    /**
        * Type of expression.
        *
        * @remarks
        * Example types include:
        *
        * - **time**: simple time expression like "3pm".
        * - **date**: simple date like "july 3rd".
        * - **datetime**: combination of date and time like "march 23 2pm".
        * - **timerange**: a range of time like "2pm to 4pm".
        * - **daterange**: a range of dates like "march 23rd to 24th".
        * - **datetimerange**: a range of dates and times like "july 3rd 2pm to 5th 4pm".
        * - **set**: a recurrence like "every monday".
        */
    type: string;
    /** Timex expressions. */
    timex: string[];
}  
```

## Run the sample

1. If you don't have the TypeScript compiler installed, install it using this command:

`npm install --global typescript`

2. Install dependencies before you run the bot, by running `npm install` in the root directory of the sample:

```
npm install
```

3. From the root directory, build the sample using `tsc`. This will generate `luisbot.js`.

4. Run `luisbot.js` in the `lib` directory.

5. Use the [Bot Framework Emulator](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-debug-emulator) to run the sample.

6. In the emulator, say `reserve a table` to start the reservation dialog.

![run the bot](graphics/run-bot.png)
