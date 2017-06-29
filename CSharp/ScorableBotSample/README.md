# ScorableBot

One of the features of using Dialogs is that it encourages developers to define a conversational hierarchy.  
This makes sense in some user scenarios where it is important to progress through a certain set of steps before reaching an outcome. 
It does, however have some limitations in that being explicit about the conversation hierarchy results in a conversation which 
is inflexible and often does not respond to the whim of the user. 

Scorables are a Bot Framework mechanism by which you can compose different parts of conversations without hardcoding the hierarchy.  
This composition allows a fluid user experience which is akin to a natural conversation

The benefits of composing chatbot conversations in this way are:
- Users can access different parts of the conversation without knowing the route to get there.
- Users do not have to 'back track' conversations.

This example demonstrates this using a 'Banking Bot' scenario, where a user may issue the command 'make payment' or 'check balance' at any
point in the conversation.

A developer wishing to implement scorable conversations should,

1. Provide 1 or more implementations of Microsoft.Bot.Builder.Scorables.Internals.ScorableBase.  See ScorableCheckBalance.cs and ScorableMakePayment.cs for examples of this.
2. Make the Autofac IOC container aware of our scorable implementations.  See Global.asax.cs for this.

In practice, the Bot Framework runtime will:

1. For each scorable implementation:
  - Call HasScore()
  - If HasScore() is true, then call GetScore()
2. Compare the results of GetScore() from each scorable implementation
3. Call PostAsync() on highest scorable 
4. Call DoneAsync() on all scorables
