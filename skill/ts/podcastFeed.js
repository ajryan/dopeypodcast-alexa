'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
var rp = require("request-promise-native");
var parsePodcast = require('node-podcast-parser');
var feedData;
function getFeed(feedUrl) {
    if (feedData) {
        console.log('return cached.');
        return new Promise(function (resolve, reject) {
            resolve(feedData);
        });
    }
    console.log('fetching new');
    return rp(feedUrl)
        .then(function (htmlString) {
        console.log('got raw xml');
        parsePodcast(htmlString, function (err, data) {
            feedData = data;
        });
        return feedData;
    })
        .catch(function (err) {
        console.log(err);
        return null;
    });
}
exports.getFeed = getFeed;
;
//# sourceMappingURL=podcastFeed.js.map