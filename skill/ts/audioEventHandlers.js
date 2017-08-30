'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
var alexaSdk = require("alexa-sdk");
var podcastFeed = require("./podcastFeed");
var constants = require("./constants");
// Binding audio handlers to PLAY_MODE State since they are expected only in this mode.
exports.playModeHandler = alexaSdk.CreateStateHandler(constants.states.PLAY_MODE, {
    'PlaybackStarted': function () {
        /*
         * AudioPlayer.PlaybackStarted Directive received.
         * Confirming that requested audio file began playing.
         * Storing details in dynamoDB using attributes.
         */
        this.attributes['token'] = getToken.call(this);
        this.attributes['index'] = getIndex.call(this);
        this.attributes['playbackFinished'] = false;
        this.emit(':saveState', true);
    },
    'PlaybackFinished': function () {
        /*
         * AudioPlayer.PlaybackFinished Directive received.
         * Confirming that audio file completed playing.
         * Storing details in dynamoDB using attributes.
         */
        this.attributes['playbackFinished'] = true;
        this.attributes['enqueuedToken'] = false;
        this.emit(':saveState', true);
    },
    'PlaybackStopped': function () {
        /*
         * AudioPlayer.PlaybackStopped Directive received.
         * Confirming that audio file stopped playing.
         * Storing details in dynamoDB using attributes.
         */
        this.attributes['token'] = getToken.call(this);
        this.attributes['index'] = getIndex.call(this);
        this.attributes['offsetInMilliseconds'] = getOffsetInMilliseconds.call(this);
        this.emit(':saveState', true);
    },
    'PlaybackNearlyFinished': function () {
        var that = this;
        var feedUrl = process.env["feed"];
        return podcastFeed.getFeed(feedUrl).then(function (feedData) {
            /*
            * AudioPlayer.PlaybackNearlyFinished Directive received.
            * Using this opportunity to enqueue the next audio
            * Storing details in dynamoDB using attributes.
            * Enqueuing the next audio file.
            */
            if (that.attributes['enqueuedToken']) {
                /*
                * Since AudioPlayer.PlaybackNearlyFinished Directive are prone to be delivered multiple times during the
                * same audio being played.
                * If an audio file is already enqueued, exit without enqueuing again.
                */
                return that.context.succeed(true);
            }
            var enqueueIndex = that.attributes['index'];
            enqueueIndex += 1;
            // Checking if  there are any items to be enqueued.
            if (enqueueIndex === feedData.episodes.length) {
                if (that.attributes['loop']) {
                    // Enqueueing the first item since looping is enabled.
                    enqueueIndex = 0;
                }
                else {
                    // Nothing to enqueue since reached end of the list and looping is disabled.
                    return that.context.succeed(true);
                }
            }
            // Setting attributes to indicate item is enqueued.
            that.attributes['enqueuedToken'] = String(that.attributes['playOrder'][enqueueIndex]);
            var enqueueToken = that.attributes['enqueuedToken'];
            var playBehavior = 'ENQUEUE';
            var podcast = feedData.episodes[that.attributes['playOrder'][enqueueIndex]];
            var expectedPreviousToken = that.attributes['token'];
            var offsetInMilliseconds = 0;
            that.response.audioPlayerPlay(playBehavior, podcast.enclosure.url, enqueueToken, expectedPreviousToken, offsetInMilliseconds);
            that.emit(':responseReady');
        });
    },
    'PlaybackFailed': function () {
        //  AudioPlayer.PlaybackNearlyFinished Directive received. Logging the error.
        console.log("Playback Failed : %j", this.event.request.error);
        this.context.succeed(true);
    }
});
function getToken() {
    // Extracting token received in the request.
    return this.event.request.token;
}
function getIndex() {
    // Extracting index from the token received in the request.
    var tokenValue = parseInt(this.event.request.token);
    return this.attributes['playOrder'].indexOf(tokenValue);
}
function getOffsetInMilliseconds() {
    // Extracting offsetInMilliseconds received in the request.
    return this.event.request.offsetInMilliseconds;
}
//# sourceMappingURL=audioEventHandlers.js.map