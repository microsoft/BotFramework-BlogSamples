const rp = require('request-promise');

function BingSearchClient (opts) {
    if (!opts.bingSearchConfig) throw new Error('bingSearchConfig is required');
    if (!opts.bingSearchKey) throw new Error('bingSearchKey is required');

    this.bingSearchConfig = opts.bingSearchConfig;
    this.bingSearchKey = opts.bingSearchKey;
    this.bingSearchCount = 6;
    this.bingSearchMkt = "en-us";
    this.bingSearchBaseUrl = "https://api.cognitive.microsoft.com/bingcustomsearch/v5.0/search";
    this.bingSearchMaxSearchStringSize = 150;
}

BingSearchClient.prototype.get = async (opts, cb) => {
    if (!opts.searchText) throw new Error('Search text is required');
    cb = cb || (() => {});

    const searchText = opts.searchText.substring(0, this.bingSearchMaxSearchStringSize).trim();

    const url = this.bingSearchBaseUrl + "?"
                + `q=${encodeURIComponent(searchText)}`
                + `&customconfig=${this.bingSearchConfig}`
                + `&count=${this.bingSearchCount}`
                + `&mkt=${this.bingSearchMkt}`
                + "&offset=0&responseFilter=Webpages&safesearch=Strict";

    const options = {
        method: 'GET',
        uri: url,
        json: true,
        headers: {
            "Ocp-Apim-Subscription-Key": this.bingSearchKey
        }
    };

    await rp(options)
        .then((body) => {
            // POST succeeded
            return cb(null, body);
        })
        .catch((err) => {
            // POST failed
            return cb(err);
        });
}

module.exports = BingSearchClient;
