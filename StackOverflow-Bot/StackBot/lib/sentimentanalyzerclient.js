const request = require('request-promise');

function SentimentAnalyzerClient (opts) {
    if (!opts.key) throw new Error('Key is required.');

    this.key = opts.key;
}

SentimentAnalyzerClient.prototype.post = async (opts, cb) => {
    if (!opts.text) throw new Error('Text is required');
    cb = cb || (() => { });

    const url = `${process.env.TEXT_ANALYTICS_URL}/sentiment`;

    const content = {
        documents: [{
            language: "en",
            id: "1",
            text: opts.text.trim()
        }]
    };

    const options = {
        method: 'POST',
        uri: url,
        body: content,
        json: true,
        headers: {
            "Ocp-Apim-Subscription-Key": this.key,
            "Content-Type": "application/json"
        }
    };

    await request(options)
        .then((body) => {
            // POST succeeded
            return cb(null, body);
        })
        .catch((err) => {
            // POST failed
            return cb(err);
        });
}

module.exports = SentimentAnalyzerClient;
