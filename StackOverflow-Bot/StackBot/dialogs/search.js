module.exports = () => {
    // Perform a search and respond with results.
    bot.dialog('sobot:search', [
        async (session, args) => {
            session.sendTyping();
            let userText = session.message.text.toLowerCase();
            bingSearchQuery(session, { query: cleanQueryString(userText, false) });
        }
    ]).triggerAction({
        matches: ['Search', 'None']
    });
}