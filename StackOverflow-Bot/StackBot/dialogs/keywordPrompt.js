module.exports = () => {
    // Ask the user what they'd like to look for.
    bot.dialog('sobot:keywordPrompt', [
        async (session, args) => {
            session.sendTyping();
            builder.Prompts.text(session, 'What would you like me to search for?');
        },
        async (session, results) => {
            session.sendTyping();
            bingSearchQuery(session, { query: cleanQueryString(results.response, false) });
        }
    ]).triggerAction({
        matches: [/I want to ask a question|Search/i]
    });
}