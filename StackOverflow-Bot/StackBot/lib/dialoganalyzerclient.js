const request = require('request-promise');
const duplex = require('stream').Duplex;

function DialogAnalyzerClient (opts) {
    if (!opts.clientId) throw new Error('Client id is required.');
    if (!opts.key) throw new Error('Key is required.');
    if (!opts.url) throw new Error('Url is required.');

    this.clientId = opts.clientId;
    this.key = opts.key;
    this.url = opts.url;
}

DialogAnalyzerClient.prototype.post = async (opts, cb) => {

    if (!opts.fileData) throw new Error('File Data is required');
    cb = cb || (() => { });

    const options = {
        method: 'POST',
        uri: this.url,
        json: true,
        headers: {
            "x-functions-clientid": this.clientId,
            "x-functions-key": this.key,
            "Content-Type": "application/octet-stream",
            "Content-Length": opts.fileData.length
        }
    };

    const stream = new duplex();
    stream.push(new Buffer(new Uint8Array(opts.fileData)));
    stream.push(null);

    await stream.pipe(request(options))
        .then((body) => {
            // POST succeeded
            return cb(null, body);
        })
        .catch((err) => {
            // POST failed
            return cb(err);
        });
}

module.exports = DialogAnalyzerClient;