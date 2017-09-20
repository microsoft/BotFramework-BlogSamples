module.exports = () => {
    // This bot can speak *all* languages!
    bot.dialog('sobot:languages', [
        async (session) => {
            session.endDialog("Hmm…. I speak JavaScript, SQL, Java, C#, Python, and more...");
        }
    ]).triggerAction({
        matches: 'Languages'
    });
}