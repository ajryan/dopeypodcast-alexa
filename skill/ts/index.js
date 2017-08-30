'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
var alexaSdk = require("alexa-sdk");
var cons = require("./constants");
var stateHandlers = require("./stateHandlers");
var audioEventHandlers = require("./audioEventHandlers");
module.exports.handler = function (event, context, callback) {
    console.log('app id is ' + cons.appId);
    var alexa = alexaSdk.handler(event, context);
    alexa.appId = cons.appId;
    alexa.dynamoDBTableName = cons.dynamoDbTableName;
    alexa.registerHandlers(stateHandlers.startModeIntentHandlers, stateHandlers.playModeIntentHandlers, stateHandlers.remoteControllerHandlers, stateHandlers.resumeDecisionModeIntentHandlers, audioEventHandlers.playModeHandler);
    alexa.execute();
};
//# sourceMappingURL=index.js.map