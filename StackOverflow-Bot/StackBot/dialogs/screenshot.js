module.exports = () => {
    // See if we can read a screenshot, and try to search the internet for help.
    bot.dialog('sobot:screenshot', [
        (session, args) => {
            // Check for reprompt
            session.dialogData.Reprompt = args.reprompt;

            // prompt user
            builder.Prompts.attachment(session, `Can you upload ${session.dialogData.Reprompt ? 'another' : 'a'} screenshot?`);
        },
        (session, results) => {

            session.sendTyping();

            var attachment = results.response[0];

            var fileDownload = checkRequiresToken(session.message)
                ? requestWithToken(attachment.contentUrl)
                : request(attachment.contentUrl, { encoding: null });

            fileDownload.then(
                (response) => {

                    session.sendTyping();

                    dialogAnalyzerClient.post({ fileData: response }, (err, res) => {
                        if (err) {
                            console.error('Error from callback:', err);
                            session.send('Oops - something went wrong.');
                            return;
                        }

                        if (res) {
                            if (res.KeyPhrases && res.KeyPhrases.length > 0) {
                                session.dialogData.KeyPhrases = res.KeyPhrases;
                            }

                            if (res.Labels && res.Labels.length > 0) {
                                session.dialogData.Labels = [];
                                for (index = 0; index < res.Labels.length; index++) {
                                    if (res.Labels[index].DialogLabelType && res.Labels[index].DialogLabelType == 1 && res.Labels[index].TextLabel && res.Labels[index].TextLabel.Text) {
                                        session.dialogData.Labels[index] = res.Labels[index].TextLabel.Text;
                                    }
                                }
                            }
                        }

                        // Ask the user to choose if both key phrases and labels exist
                        if (session.dialogData.KeyPhrases && session.dialogData.Labels) {
                            session.send("I found the following key phrases in the screeshot that you have uploaded...\n\r" + session.dialogData.KeyPhrases.join(", "));
                            builder.Prompts.choice(session, "Shall I", "Search the full text|Search the key phrases", { listStyle: 2 });
                        }
                        // Process key phrases if only key phrases exist
                        else if (session.dialogData.KeyPhrases) {
                            var q = cleanQueryString(session.dialogData.KeyPhrases.join(" "), true);
                            var ftq = cleanQueryString(session.dialogData.Labels.join(" "), true);
                            bingSearchQuery(session, { query: q, fullTextQuery: ftq });
                        }
                        // Process labels if only labels exist
                        else if (session.dialogData.Labels) {
                            var ftq = cleanQueryString(session.dialogData.Labels.join(" "), true);
                            bingSearchQuery(session, { query: ftq });
                        }
                        else {
                            var notErrorMsg = "Hummm... This does not look like an error.";

                            if (res.Captions && res.Captions.length > 0) {
                                notErrorMsg += ` It looks like ${res.Captions[0]}.`;
                            }

                            if (session.dialogData.Reprompt) {
                                return session.endDialog(notErrorMsg);
                            }
                            else {
                                session.send(notErrorMsg);
                                session.replaceDialog('screenshot', { reprompt: true })
                            }
                        }
                    });

                }).catch((err) => {
                    return session.endDialog('Oops. Error reading file.');
                });
        },
        (session, results) => {
            if (results.response) {
                var q;
                var ftq = cleanQueryString(session.dialogData.Labels.join(" "), true);
                switch (results.response.index) {
                    case 0:
                        q = ftq;
                        break;
                    case 1:
                        q = cleanQueryString(session.dialogData.KeyPhrases.join(" "), true);
                        break;
                    default:
                        return session.endDialog("Oops - something went wrong.");
                }

                searchQuery(session, { query: q, fullTextQuery: ftq });
            } else {
                session.beginDialog('smalltalk');
            }
        }
    ]).triggerAction({
        matches: [/screenshot|dialog/i]
    });

    const checkRequiresToken = (message) => {
        return message.source === 'skype' || message.source === 'msteams';
    };

    // Request file with Authentication Header
    const requestWithToken = (url) => {
        return obtainToken().then((token) => {
            return request({
                url: url,
                headers: {
                    'Authorization': 'Bearer ' + token,
                    'Content-Type': 'application/octet-stream'
                },
                encoding: null
            });
        });
    };
}