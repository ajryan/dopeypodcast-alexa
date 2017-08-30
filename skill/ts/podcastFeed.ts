'use strict';

import * as rp from 'request-promise-native';
var parsePodcast = require('node-podcast-parser');

var feedData: any;

export function getFeed(feedUrl: string) : Promise<any> {
  if (feedData) {
    console.log('return cached.');
    return new Promise<any>((resolve, reject) => {
      resolve(feedData);
    });
  }

  console.log('fetching new');

  return rp(feedUrl)
    .then(htmlString => {
      console.log('got raw xml');
      parsePodcast(htmlString, (err, data) => {
        feedData = data;
      });
      return feedData;
    })
    .catch(err => {
      console.log(err);
      return null;
    });
};