var assert = require('assert');
var podcastFeed = require('../podcastFeed')
describe('podcatFeed returns expected title', function() {
    it('simple assert', function() {
        return podcastFeed.getFeed('http://dopeypodcast.podbean.com/feed').then(function(feed) {
            assert.equal(feed.title.substring(0, 5), 'Dopey');
        });
    });
});