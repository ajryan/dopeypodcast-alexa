using System;
using System.Collections.Generic;
using System.Linq;

using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;

using Amazon.Lambda.Core;

using skill.net.PodcastFeed;

namespace skill.net
{
  public class AudioController
  {
    private readonly List<FeedItem> _feedItems;
    private readonly ILambdaLogger _logger;

    public AudioController(List<FeedItem> feedItems, ILambdaLogger logger)
    {
      _feedItems = feedItems;
      _logger = logger;
    }

    public SkillResponse Play(AudioSessionAttributes attributes, bool isResetPlayOrder, string expectedPreviousToken = null)
    {
      _logger.LogLine($"AudioController.Play isResetPlayOrder={isResetPlayOrder}, expectedPreviousToken={expectedPreviousToken}");

      attributes.State = Constants.States.PlayMode;

      if (isResetPlayOrder)
      {
        _logger.LogLine("Resetting play order.");

        var currentItem = _feedItems[attributes.Index];

        attributes.PlayOrder = _feedItems
          .OrderByDescending(aItem => aItem.PublishDate)
          .Select(item => _feedItems.IndexOf(item)).ToList();

        if (!attributes.PlaybackFinished)
          attributes.Index = _feedItems.IndexOf(currentItem);
      }

      if (attributes.PlaybackFinished)
      {
        _logger.LogLine("Playback is finished.");

        // Reset to top of the playlist when reached end.
        attributes.Index = 0;
        attributes.OffsetInMilliseconds = 0;
        attributes.PlaybackIndexChanged = true;
        attributes.PlaybackFinished = false;
      }

      var token = attributes.PlayOrder[attributes.Index];
      var feedItem = _feedItems[attributes.Index];
      var mp3Url = feedItem.Mp3Url.Replace("http://", "https://");
      var artUrl = feedItem.ArtUrl.Replace("http://", "https://");

      attributes.EnqueuedToken = -1;

      _logger.LogLine($"Building player response for {mp3Url}.");

      var response = ResponseBuilder.AudioPlayerPlay(
        expectedPreviousToken == null ? PlayBehavior.ReplaceAll : PlayBehavior.Enqueue,
        mp3Url,
        token.ToString(),
        expectedPreviousToken,
        (int)attributes.OffsetInMilliseconds);

      response.Response.ShouldEndSession = false;

      if (attributes.PlaybackIndexChanged)
      {
        _logger.LogLine("Playback index changed - adding card.");

        var cardTitle = $"Playing {feedItem.Title}";
        var cardContent = feedItem.Description;

        response.Response.Card = new StandardCard
        {
          Title = cardTitle,
          Content = cardContent,
          Image = new CardImage { SmallImageUrl = artUrl, LargeImageUrl = artUrl }
        };
      }

      return response;
    }

    public SkillResponse PlaybackNearlyFinished(AudioSessionAttributes attributes)
    {
      if (attributes.EnqueuedToken != -1) {
        /*
        * Since AudioPlayer.PlaybackNearlyFinished Directive are prone to be delivered multiple times during the
        * same audio being played.
        * If an audio file is already enqueued, exit without enqueuing again.
        */
        return Speech.GetContinueResponse();
      }

      int enqueueIndex = attributes.Index + 1;

      // Checking if  there are any items to be enqueued.
      if (enqueueIndex == _feedItems.Count) {
        if (attributes.Loop) {
          // Enqueueing the first item since looping is enabled.
          enqueueIndex = 0;
        } else {
          // Nothing to enqueue since reached end of the list and looping is disabled.
          return Speech.GetContinueResponse();
        }
      }
      // Setting attributes to indicate item is enqueued.
      attributes.EnqueuedToken = attributes.PlayOrder[enqueueIndex];

      var expectedPreviousToken = attributes.PlayOrder[attributes.Index].ToString();
      attributes.OffsetInMilliseconds = 0;
      attributes.Index = enqueueIndex;
      return Play(attributes, false, expectedPreviousToken);
    }

    public SkillResponse Next(AudioSessionAttributes attributes)
    {
      return Increment(attributes, 1);
    }

    public SkillResponse Previous(AudioSessionAttributes attributes)
    {
      return Increment(attributes, -1);
    }

    public SkillResponse Stop(AudioSessionAttributes attributes)
    {
      return ResponseBuilder.AudioPlayerStop();
    }

    public SkillResponse Loop(AudioSessionAttributes attributes, bool loopEnable)
    {
      attributes.Loop = loopEnable;

      return Speech.GetTellResponse($"Loop turned {(loopEnable ? "on" : "off")}", false);
    }

    public SkillResponse Shuffle(AudioSessionAttributes attributes, bool shuffleEnable)
    {
      attributes.Shuffle = shuffleEnable;

      if (!shuffleEnable)
        return Play(attributes, true);

      var playOrder = Enumerable.Range(0, _feedItems.Count).ToList();
      var random = new Random();
      int currentIndex = playOrder.Count - 1;

      while (currentIndex >= 0)
      {
        int randomIndex = (int)(random.NextDouble() * currentIndex);
        int temp = playOrder[currentIndex];
        playOrder[currentIndex] = playOrder[randomIndex];
        playOrder[randomIndex] = temp;

        currentIndex--;
      }

      attributes.PlayOrder = playOrder;
      attributes.Index = 0;
      attributes.OffsetInMilliseconds = 0;
      attributes.PlaybackIndexChanged = true;

      return Play(attributes, false);
    }

    private SkillResponse Increment(AudioSessionAttributes attributes, int offset)
    {
      int index = attributes.Index + offset;

      if (index == _feedItems.Count || index == -1)
      {
        if (attributes.Loop)
          index = offset > 0 ? 0 : _feedItems.Count - 1;
        else
        {
          attributes.State = Constants.States.StartMode;

          var response = ResponseBuilder.AudioPlayerStop();
          Speech.AddOutputSpeech(response, $"You have reached the {(offset > 0 ? "last" : "first")} episode.");
          return response;
        }
      }

      attributes.Index = index;
      attributes.OffsetInMilliseconds = 0;
      attributes.PlaybackIndexChanged = true;

      return Play(attributes, false);
    }
  }
}
