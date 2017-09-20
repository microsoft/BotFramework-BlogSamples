module.exports = () => {
    // Shows off a menu with all the capabilities.
    bot.dialog('sobot:menu', [
        async (session) => {
            let msg = attachments.buildMenuMessageWithAttachments(session);
            session.endDialog(msg);
        }
    ]).triggerAction({
        matches: 'Help'
    });
}
