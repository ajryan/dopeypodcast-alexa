'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
var alexaSdk = require("alexa-sdk");
var podcastFeed = require("./podcastFeed");
var constants = require("./constants");
exports.startModeIntentHandlers = alexaSdk.CreateStateHandler(constants.states.START_MODE, {
    /*
     *  All Intent Handlers for state : START_MODE
     */
    'LaunchRequest': function () {
        var that = this;
        var feedUrl = process.env["feed"];
        return podcastFeed.getFeed(feedUrl).then(function (feed) {
            console.log(feed);
            // Initialize Attributes
            that.attributes['playOrder'] = Array.apply(null, { length: feed.episodes.length }).map(Number.call, Number);
            that.attributes['index'] = 0;
            that.attributes['offsetInMilliseconds'] = 0;
            that.attributes['loop'] = true;
            that.attributes['shuffle'] = false;
            that.attributes['playbackIndexChanged'] = true;
            //  Change state to START_MODE
            that.handler.state = constants.states.START_MODE;
            var message = 'Welcome to the Dopey Podcast. You can say, play the audio, to begin the podcast.';
            var reprompt = 'You can say, play the audio, to begin.';
            that.response.speak(message).listen(reprompt);
            that.emit(':responseReady');
        });
    },
    'PlayAudio': function () {
        var that = this;
        var feedUrl = process.env["feed"];
        console.log(process.env["feed"]);
        return podcastFeed.getFeed(feedUrl).then(function (feed) {
            console.log(feed);
            if (!that.attributes['playOrder']) {
                // Initialize Attributes if undefined.
                that.attributes['playOrder'] = Array.apply(null, { length: feed.episodes.length }).map(Number.call, Number);
                that.attributes['index'] = 0;
                that.attributes['offsetInMilliseconds'] = 0;
                that.attributes['loop'] = true;
                that.attributes['shuffle'] = false;
                that.attributes['playbackIndexChanged'] = true;
                //  Change state to START_MODE
                that.handler.state = constants.states.START_MODE;
            }
            return controller.play.call(that);
        });
    },
    'AMAZON.HelpIntent': function () {
        var message = 'Welcome to the AWS Podcast. You can say, play the audio, to begin the podcast.';
        this.response.speak(message).listen(message);
        this.emit(':responseReady');
    },
    'AMAZON.StopIntent': function () {
        var message = 'Good bye.';
        this.response.speak(message);
        this.emit(':responseReady');
    },
    'AMAZON.CancelIntent': function () {
        var message = 'Good bye.';
        this.response.speak(message);
        this.emit(':responseReady');
    },
    'SessionEndedRequest': function () {
        // No session ended logic
    },
    'Unhandled': function () {
        var message = 'Sorry, I could not understand. Please say, play the audio, to begin the audio.';
        this.response.speak(message).listen(message);
        this.emit(':responseReady');
    }
});
exports.playModeIntentHandlers = alexaSdk.CreateStateHandler(constants.states.PLAY_MODE, {
    /*
     *  All Intent Handlers for state : PLAY_MODE
     */
    'LaunchRequest': function () {
        /*
         *  Session resumed in PLAY_MODE STATE.
         *  If playback had finished during last session :
         *      Give welcome message.
         *      Change state to START_STATE to restrict user inputs.
         *  Else :
         *      Ask user if he/she wants to resume from last position.
         *      Change state to RESUME_DECISION_MODE
         */
        var message;
        var reprompt;
        if (this.attributes['playbackFinished']) {
            this.handler.state = constants.states.START_MODE;
            message = 'Welcome to the AWS Podcast. You can say, play the audio to begin the podcast.';
            reprompt = 'You can say, play the audio, to begin.';
        }
        else {
            this.handler.state = constants.states.RESUME_DECISION_MODE;
            message = 'You were listening to episode ' + (this.attributes['index'] + 1) + '. Would you like to resume?';
            reprompt = 'You can say yes to resume or no to play from the top.';
        }
        this.response.speak(message).listen(reprompt);
        this.emit(':responseReady');
    },
    'PlayAudio': function () { controller.play.call(this); },
    'AMAZON.NextIntent': function () { controller.playNext.call(this); },
    'AMAZON.PreviousIntent': function () { controller.playPrevious.call(this); },
    'AMAZON.PauseIntent': function () { controller.stop.call(this); },
    'AMAZON.StopIntent': function () { controller.stop.call(this); },
    'AMAZON.CancelIntent': function () { controller.stop.call(this); },
    'AMAZON.ResumeIntent': function () { controller.play.call(this); },
    'AMAZON.LoopOnIntent': function () { controller.loopOn.call(this); },
    'AMAZON.LoopOffIntent': function () { controller.loopOff.call(this); },
    'AMAZON.ShuffleOnIntent': function () { controller.shuffleOn.call(this); },
    'AMAZON.ShuffleOffIntent': function () { controller.shuffleOff.call(this); },
    'AMAZON.StartOverIntent': function () { controller.startOver.call(this); },
    'AMAZON.HelpIntent': function () {
        // This will called while audio is playing and a user says "ask <invocation_name> for help"
        var message = 'You are listening to the AWS Podcast. You can say, Next or Previous to navigate through the playlist. ' +
            'At any time, you can say Pause to pause the audio and Resume to resume.';
        this.response.speak(message).listen(message);
        this.emit(':responseReady');
    },
    'SessionEndedRequest': function () {
        // No session ended logic
    },
    'Unhandled': function () {
        var message = 'Sorry, I could not understand. You can say, Next or Previous to navigate through the playlist.';
        this.response.speak(message).listen(message);
        this.emit(':responseReady');
    }
});
exports.remoteControllerHandlers = alexaSdk.CreateStateHandler(constants.states.PLAY_MODE, {
    /*
     *  All Requests are received using a Remote Control. Calling corresponding handlers for each of them.
     */
    'PlayCommandIssued': function () { controller.play.call(this); },
    'PauseCommandIssued': function () { controller.stop.call(this); },
    'NextCommandIssued': function () { controller.playNext.call(this); },
    'PreviousCommandIssued': function () { controller.playPrevious.call(this); }
});
exports.resumeDecisionModeIntentHandlers = alexaSdk.CreateStateHandler(constants.states.RESUME_DECISION_MODE, {
    /*
     *  All Intent Handlers for state : RESUME_DECISION_MODE
     */
    'LaunchRequest': function () {
        var message = 'You were listening to episode ' + (this.attributes['index'] + 1) + '. Would you like to resume?';
        var reprompt = 'You can say yes to resume or no to play from the top.';
        this.response.speak(message).listen(reprompt);
        this.emit(':responseReady');
    },
    'AMAZON.YesIntent': function () { controller.play.call(this); },
    'AMAZON.NoIntent': function () { controller.reset.call(this); },
    'AMAZON.HelpIntent': function () {
        var message = 'You were listening to episode ' + (this.attributes['index'] + 1) + '. Would you like to resume?';
        var reprompt = 'You can say yes to resume or no to play from the top.';
        this.response.speak(message).listen(reprompt);
        this.emit(':responseReady');
    },
    'AMAZON.StopIntent': function () {
        var message = 'Good bye.';
        this.response.speak(message);
        this.emit(':responseReady');
    },
    'AMAZON.CancelIntent': function () {
        var message = 'Good bye.';
        this.response.speak(message);
        this.emit(':responseReady');
    },
    'SessionEndedRequest': function () {
        // No session ended logic
    },
    'Unhandled': function () {
        var message = 'Sorry, this is not a valid command. Please say help to hear what you can say.';
        this.response.speak(message).listen(message);
        this.emit(':responseReady');
    }
});
var controller = function () {
    return {
        play: function (feed) {
            /*
             *  Using the function to begin playing audio when:
             *      Play Audio intent invoked.
             *      Resuming audio when stopped/paused.
             *      Next/Previous commands issued.
             */
            this.handler.state = constants.states.PLAY_MODE;
            if (this.attributes['playbackFinished']) {
                // Reset to top of the playlist when reached end.
                this.attributes['index'] = 0;
                this.attributes['offsetInMilliseconds'] = 0;
                this.attributes['playbackIndexChanged'] = true;
                this.attributes['playbackFinished'] = false;
            }
            var token = String(this.attributes['playOrder'][this.attributes['index']]);
            var playBehavior = 'REPLACE_ALL';
            var podcast = feed[this.attributes['playOrder'][this.attributes['index']]];
            var offsetInMilliseconds = this.attributes['offsetInMilliseconds'];
            // Since play behavior is REPLACE_ALL, enqueuedToken attribute need to be set to null.
            this.attributes['enqueuedToken'] = null;
            if (canThrowCard.call(this)) {
                var cardTitle = 'Playing ' + podcast.title;
                var cardContent = 'Playing ' + podcast.title + ': ' + podcast.description;
                this.response.cardRenderer(cardTitle, cardContent, null);
            }
            this.response.audioPlayerPlay(playBehavior, podcast.enclosure.url, token, null, offsetInMilliseconds);
            this.emit(':responseReady');
        },
        stop: function () {
            /*
             *  Issuing AudioPlayer.Stop directive to stop the audio.
             *  Attributes already stored when AudioPlayer.Stopped request received.
             */
            this.response.audioPlayerStop();
            this.emit(':responseReady');
        },
        playNext: function () {
            /*
             *  Called when AMAZON.NextIntent or PlaybackController.NextCommandIssued is invoked.
             *  Index is computed using token stored when AudioPlayer.PlaybackStopped command is received.
             *  If reached at the end of the playlist, choose behavior based on "loop" flag.
             */
            var that = this;
            var index = this.attributes['index'];
            index += 1;
            // Check for last audio file.
            var feedUrl = process.env["feed"];
            return podcastFeed.getFeed(feedUrl).then(function (feed) {
                if (index === feed.episodes.length) {
                    if (that.attributes['loop']) {
                        index = 0;
                    }
                    else {
                        // Reached at the end. Thus reset state to start mode and stop playing.
                        that.handler.state = constants.states.START_MODE;
                        var message = 'You have reached at the end of the playlist.';
                        that.response.speak(message).audioPlayerStop();
                        return that.emit(':responseReady');
                    }
                }
                // Set values to attributes.
                that.attributes['index'] = index;
                that.attributes['offsetInMilliseconds'] = 0;
                that.attributes['playbackIndexChanged'] = true;
                controller.play.call(that);
            });
        },
        playPrevious: function () {
            /*
             *  Called when AMAZON.PreviousIntent or PlaybackController.PreviousCommandIssued is invoked.
             *  Index is computed using token stored when AudioPlayer.PlaybackStopped command is received.
             *  If reached at the end of the playlist, choose behavior based on "loop" flag.
             */
            var that = this;
            var index = this.attributes['index'];
            index -= 1;
            var feedUrl = process.env["feed"];
            return podcastFeed.getFeed(feedUrl).then(function (feed) {
                // Check for last audio file.
                if (index === -1) {
                    if (that.attributes['loop']) {
                        index = feed.episodes.length - 1;
                    }
                    else {
                        // Reached at the end. Thus reset state to start mode and stop playing.
                        that.handler.state = constants.states.START_MODE;
                        var message = 'You have reached at the start of the playlist.';
                        that.response.speak(message).audioPlayerStop();
                        return that.emit(':responseReady');
                    }
                }
                // Set values to attributes.
                that.attributes['index'] = index;
                that.attributes['offsetInMilliseconds'] = 0;
                that.attributes['playbackIndexChanged'] = true;
                controller.play.call(that);
            });
        },
        loopOn: function () {
            // Turn on loop play.
            this.attributes['loop'] = true;
            var message = 'Loop turned on.';
            this.response.speak(message);
            this.emit(':responseReady');
        },
        loopOff: function () {
            // Turn off looping
            this.attributes['loop'] = false;
            var message = 'Loop turned off.';
            this.response.speak(message);
            this.emit(':responseReady');
        },
        shuffleOn: function () {
            var feedUrl = process.env["feed"];
            return podcastFeed.getFeed(feedUrl).then(function (feed) {
                var _this = this;
                // Turn on shuffle play.
                this.attributes['shuffle'] = true;
                shuffleOrder(feed, function (newOrder) {
                    // Play order have been shuffled. Re-initializing indices and playing first song in shuffled order.
                    _this.attributes['playOrder'] = newOrder;
                    _this.attributes['index'] = 0;
                    _this.attributes['offsetInMilliseconds'] = 0;
                    _this.attributes['playbackIndexChanged'] = true;
                    controller.play.call(_this);
                });
            });
        },
        shuffleOff: function () {
            // Turn off shuffle play. 
            var that = this;
            var feedUrl = process.env["feed"];
            return podcastFeed.getFeed(feedUrl).then(function (feed) {
                if (that.attributes['shuffle']) {
                    that.attributes['shuffle'] = false;
                    // Although changing index, no change in audio file being played as the change is to account for reordering playOrder
                    that.attributes['index'] = that.attributes['playOrder'][that.attributes['index']];
                    that.attributes['playOrder'] = Array.apply(null, { length: feed.episodes.length }).map(Number.call, Number);
                }
                controller.play.call(that);
            });
        },
        startOver: function () {
            // Start over the current audio file.
            this.attributes['offsetInMilliseconds'] = 0;
            controller.play.call(this);
        },
        reset: function () {
            // Reset to top of the playlist.
            this.attributes['index'] = 0;
            this.attributes['offsetInMilliseconds'] = 0;
            this.attributes['playbackIndexChanged'] = true;
            controller.play.call(this);
        }
    };
}();
function canThrowCard() {
    /*
     * To determine when can a card should be inserted in the response.
     * In response to a PlaybackController Request (remote control events) we cannot issue a card,
     * Thus adding restriction of request type being "IntentRequest".
     */
    if (this.event.request.type === 'IntentRequest' && this.attributes['playbackIndexChanged']) {
        this.attributes['playbackIndexChanged'] = false;
        return true;
    }
    else {
        return false;
    }
}
function shuffleOrder(feed, callback) {
    // Algorithm : Fisher-Yates shuffle
    var array = Array.apply(null, { length: feed.episodes.length }).map(Number.call, Number);
    var currentIndex = array.length;
    var temp, randomIndex;
    while (currentIndex >= 1) {
        randomIndex = Math.floor(Math.random() * currentIndex);
        currentIndex -= 1;
        temp = array[currentIndex];
        array[currentIndex] = array[randomIndex];
        array[randomIndex] = temp;
    }
    callback(array);
}
//# sourceMappingURL=stateHandlers.js.map