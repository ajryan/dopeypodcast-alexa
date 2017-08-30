'use strict';

import * as alexaSdk from 'alexa-sdk';
import * as cons from './constants';
import * as stateHandlers from './stateHandlers';
import * as audioEventHandlers from './audioEventHandlers';

module.exports.handler = (event, context: alexaSdk.Context, callback) => {
  console.log('app id is ' + cons.appId);
  var alexa = alexaSdk.handler(event, context);
  alexa.appId = cons.appId;
  alexa.dynamoDBTableName = cons.dynamoDbTableName;
  alexa.registerHandlers(
    stateHandlers.startModeIntentHandlers,
    stateHandlers.playModeIntentHandlers,
    stateHandlers.remoteControllerHandlers,
    stateHandlers.resumeDecisionModeIntentHandlers,
    audioEventHandlers.playModeHandler
  );
  alexa.execute();
};