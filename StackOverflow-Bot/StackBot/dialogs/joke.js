module.exports = () => {
    // Tells the user a fun joke.
    bot.dialog('sobot:joke', [
        (session) => {
            session.send("Here's a fun joke… 🙃");
            tellAJoke(session);
            session.endDialog("Feel free to ask me another question, or even ask for a joke!");
            return;
        }
    ]).triggerAction({
        matches: ['Joke', /^Brighten my day/i]
    });

    const tellAJoke = (session) => {
        let usedJoke = pickAJoke(jokes.items);
        session.send(usedJoke);
        return;
    }
    
    const pickAJoke = (jokes) => {
        let randomIndex = getRandomInt(0, jokes.length);
        return jokes[randomIndex].body_markdown;
    }
    
    const getRandomInt = (min, max) => {
        min = Math.ceil(min);
        max = Math.floor(max);
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }
}   