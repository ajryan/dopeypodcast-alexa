'use strict';

var rp = require('request-promise');
var parsePodcast = require('node-podcast-parser');
var feedData;

exports.getFeed = function(feedUrl) {
    if (feedData) {
        console.log('return cached.');
        return new Promise(function(resolve, reject) {
            resolve(feedData);
        });
    }

    console.log('fetching new');
    return rp(feedUrl)
        .then(function(htmlString) {
            console.log('got raw xml');
            parsePodcast(htmlString, (err, data) => {
                feedData = data;
            });
            return feedData;
        })
        .catch(function(err) {
            console.log(err);
            return null;
        });
};