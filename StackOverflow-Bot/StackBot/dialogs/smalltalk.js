module.exports = () => {
    // Smalltalk… gibberish, babble, etc. Not the programming language :)
    bot.dialog('sobot:smalltalk', [
        async (session) => {
            // Start async tasks
            const sentimentTask = fetchSentiment(session.message.text);
            const qnATask = fetchQnA(session.message.text);

            // Wait for tasks to complete
            const sentimentScore = await sentimentTask;
            const qnAResponse = await qnATask;

            const sentimentThreshold = 0.25;

            // Check to see if our user seems a little bit frustrated, and would ather hear a joke.
            if (sentimentScore && sentimentScore < sentimentThreshold) {
                builder.Prompts.confirm(session, "I'm sorry about that, would you like to hear a joke instead?");
            } else if (qnAResponse) {
                // Continue with the smalltalk.
                return session.endDialog(qnAResponse);
            } else {
                // Else the user wants to search bing.
                bingSearchQuery(session, { query: cleanQueryString(session.message.text, false) });
            }
        },
        (session, results) => {
            if (results.response) {
                session.beginDialog('joke');
            } else {
                return session.endDialog("Ok then. Feel free to ask me another question, or even ask for a joke!");
            }
        }
    ]).triggerAction({
        matches: 'SmallTalk'
    });

    const fetchQnA = async (text) => {
        let answer;

        await qnaClient.post({ question: text }, (err, res) => {
            if (err) {
                console.error('Error from callback:', err);
            } else if (res) {
                answer = res;
            }
        });

        return answer;
    }

    const fetchSentiment = async (text) => {
        let score;

        await sentimentAnalyzerClient.post({ text: text }, (err, res) => {
            if (err) {
                console.error('Error from callback:', err);
            } else if (res && res.documents && res.documents.length > 0) {
                score = res.documents[0].score;
            }
        });

        return score;
    }
}