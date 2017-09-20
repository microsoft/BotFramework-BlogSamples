const rp = require('request-promise');
const smallTalkReplies = require('./smalltalk');
const QNA_MAKER_URL = `${process.env.QNA_URL}/knowledgebases`;

function Client (opts) {
    if (!opts.knowledgeBaseId) throw new Error('knowledgeBaseId is required');
    if (!opts.subscriptionKey) throw new Error('subscriptionKey is required');

    this.knowledgeBaseId = opts.knowledgeBaseId;
    this.subscriptionKey = opts.subscriptionKey;
    this.scoreThreshold = opts.scoreThreshold ? opts.scoreThreshold : 20; // 20 is the default
}

Client.prototype.post = async (opts, cb) => {
    if (!opts.question) throw new Error('question is required');
    cb = cb || (() => { });

    const url = `${QNA_MAKER_URL}/${this.knowledgeBaseId}/generateAnswer`;

    const options = {
        method: 'POST',
        uri: url,
        json: true,
        body: opts,
        headers: {
            "Ocp-Apim-Subscription-Key": this.subscriptionKey,
            "Content-Type": "application/json"
        }
    };

    await rp(options)
        .then((body) => {
            // POST succeeded
            let botreply;
            const answerobj = body.answers[0];

            if (answerobj.score >= self.scoreThreshold) {
                // Answer confidence score is acceptable - use QnA maker's response
                const botreplylist = smallTalkReplies[answerobj.answer];
                botreply = botreplylist[Math.floor(Math.random() * botreplylist.length)];
            }

            return cb(null, botreply);
        })
        .catch((err) => {
            // POST failed
            return cb(err);
        });
}

module.exports = Client;
