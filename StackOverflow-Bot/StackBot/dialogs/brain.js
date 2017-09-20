module.exports = () => {
    bot.dialog('sobot:brain', [
        async (session) => {
            session.send("Here are the Cognitive services that I use to help you today...");
            var servicesMessage = attachments.buildCongitiveServicesMessageWithAttachments(session);
            return session.endDialog(servicesMessage);
        }
    ]).triggerAction({
        matches: 'Brain'
    });
}