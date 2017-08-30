using System;

using Alexa.NET;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

using Amazon.Lambda.Core;

using skill.net.PodcastFeed;

namespace skill.net
{
  public class PodcastAudioStateEventHandler : IStateEventHandler<AudioSessionAttributes>
  {
    private readonly ILambdaLogger _logger;

    public PodcastAudioStateEventHandler(ILambdaLogger logger)
    {
      _logger = logger;
    }

    public SkillResponse HandleStateEvent(string state, string @event, AudioSessionAttributes attributes, ILambdaContext context, Request request)
    {
      switch (state)
      {
        case Constants.States.StartMode:
        {
          switch (@event)
          {
            case Constants.Events.LaunchRequest:       return LaunchRequest(attributes);
            case Constants.Events.PlayAudio:           return PlayAudio(attributes, true);
            case Constants.Events.Amazon.HelpIntent:   return Help(attributes);
            case Constants.Events.Amazon.StopIntent:   return StopCancel(attributes);
            case Constants.Events.Amazon.CancelIntent: return StopCancel(attributes);
            case Constants.Events.SessionEndedRequest: return SessionEnded(attributes);
            case Constants.Events.Unhandled:           return Unhandled(attributes);
            default: throw new InvalidOperationException($"Can't handle state {state} event {@event}");
          }
        }

        case Constants.States.PlayMode:
        {
          switch (@event)
          {
            case Constants.Events.LaunchRequest:          return LaunchRequest(attributes);
            case Constants.Events.PlayAudio:              return PlayAudio(attributes, false);
            case Constants.Events.Amazon.NextIntent:      return NextAudio(attributes);
            case Constants.Events.Amazon.PreviousIntent:  return PreviousAudio(attributes);
            case Constants.Events.Amazon.PauseIntent:     return StopAudio(attributes);
            case Constants.Events.Amazon.StopIntent:      return StopAudio(attributes);
            case Constants.Events.Amazon.CancelIntent:    return StopAudio(attributes);
            case Constants.Events.Amazon.ResumeIntent:    return PlayAudio(attributes, false);
            case Constants.Events.Amazon.LoopOnIntent:    return LoopAudio(attributes, true);
            case Constants.Events.Amazon.LoopOffIntent:   return LoopAudio(attributes, false);
            case Constants.Events.Amazon.ShuffleOnIntent: return ShuffleAudio(attributes, true);
            case Constants.Events.Amazon.ShuffleOffInten: return ShuffleAudio(attributes, false);
            case Constants.Events.Amazon.StartOverIntent: return StartOverAudio(attributes);
            case Constants.Events.Amazon.HelpIntent:      return HelpWhilePlaying(attributes);
            case Constants.Events.SessionEndedRequest:    return SessionEnded(attributes);
            case Constants.Events.Unhandled:              return UnhandledWhilePlaying(attributes);

            // Remote controller handlers
            case Constants.Events.PlayCommandIssued:      return PlayAudio(attributes, false);
            case Constants.Events.PauseCommandIssued:     return StopAudio(attributes);
            case Constants.Events.NextCommandIssued:      return NextAudio(attributes);
            case Constants.Events.PreviousCommandIssued:  return PreviousAudio(attributes);

              // Audio events
            case Constants.Events.Audio.PlaybackStarted:
            {
              attributes.PlaybackFinished = false;
              attributes.Index = GetIndexFromToken(attributes, ((AudioPlayerRequest) request).Token);
              return Speech.GetContinueResponse();
            }

            case Constants.Events.Audio.PlaybackFinished:
            {
              attributes.PlaybackFinished = true;
              attributes.EnqueuedToken = -1;
              return Speech.GetContinueResponse();
            }

            case Constants.Events.Audio.PlaybackStopped:
            {
              attributes.Index = GetIndexFromToken(attributes, ((AudioPlayerRequest) request).Token);
              attributes.OffsetInMilliseconds = ((AudioPlayerRequest)request).OffsetInMilliseconds;
              return Speech.GetContinueResponse();
            }

            case Constants.Events.Audio.PlaybackNearlyFinished:
              return PlaybackNearlyFinishedAudio(attributes);

            case Constants.Events.Audio.PlaybackFailed:
            {
              _logger.LogLine($"Playback failed: {((AudioPlayerRequest)request).Error}");
              return Speech.GetContinueResponse();
            }

            default: throw new InvalidOperationException($"Can't handle state {state} event {@event}");
          }
        }

        case Constants.States.ResumeDecisionMode:
        {
          switch (@event)
          {
              case Constants.Events.LaunchRequest:
              case Constants.Events.Amazon.HelpIntent:
                return Speech.GetAskResponse(
                  $"You were listening to episode {attributes.Index + 1}. Would you like to resume?",
                  "You can say yes to resume or no to play from the top.");

              case Constants.Events.Amazon.YesIntent: return PlayAudio(attributes, false);
              case Constants.Events.Amazon.NoIntent: return StartOverAudio(attributes);

              case Constants.Events.Amazon.StopIntent:
              case Constants.Events.Amazon.CancelIntent:
                return StopAudio(attributes);

            case Constants.Events.SessionEndedRequest: return SessionEnded(attributes);
            case Constants.Events.Unhandled: return UnhandledWhilePlaying(attributes);

            default: throw new InvalidOperationException($"Can't handle state {state} event {@event}");
          }
        }
      }

      throw new InvalidOperationException($"Can't handle state {state} event {@event}");
    }

    internal SkillResponse LaunchRequest(AudioSessionAttributes attributes)
    {
      attributes.Index = 0;
      attributes.VisitCount++;
      attributes.OffsetInMilliseconds = 0;
      attributes.Loop = true;
      attributes.Shuffle = false;
      attributes.PlaybackIndexChanged = true;
      attributes.EnqueuedToken = -1;

      attributes.State = Constants.States.StartMode;

      var response = Speech.GetAskResponse(
        "Welcome to the Dopey Podcast. You can say, play the audio, to begin the podcast.",
        "You can say, play the audio, to begin.");

      response.Response.Card = new StandardCard
      {
        Title = "Dopy Podcast",
        Content = "The dark comedy of drug addiction.",
        Image = new CardImage
        {
          LargeImageUrl = "https://deow9bq0xqvbj.cloudfront.net/image-logo/949223/dopeyreal.jpg",
          SmallImageUrl = "https://deow9bq0xqvbj.cloudfront.net/image-logo/949223/dopeyreal.jpg"
        }
      };

      return response;
    }

    internal SkillResponse PlayAudio(AudioSessionAttributes attributes, bool isStartState)
    {
      return GetAudioController().Play(attributes, isStartState);
    }

    internal SkillResponse PlaybackNearlyFinishedAudio(AudioSessionAttributes attributes)
    {
      return GetAudioController().PlaybackNearlyFinished(attributes);
    }

    internal SkillResponse NextAudio(AudioSessionAttributes attributes)
    {
      return GetAudioController().Next(attributes);
    }

    internal SkillResponse PreviousAudio(AudioSessionAttributes attributes)
    {
      return GetAudioController().Previous(attributes);
    }

    internal SkillResponse StopAudio(AudioSessionAttributes attributes)
    {
      return GetAudioController().Stop(attributes);
    }

    internal SkillResponse LoopAudio(AudioSessionAttributes attributes, bool loopEnable)
    {
      return GetAudioController().Loop(attributes, loopEnable);
    }

    internal SkillResponse ShuffleAudio(AudioSessionAttributes attributes, bool shuffleEnable)
    {
      return GetAudioController().Shuffle(attributes, shuffleEnable);
    }

    internal SkillResponse StartOverAudio(AudioSessionAttributes attributes)
    {
      attributes.OffsetInMilliseconds = 0;
      return GetAudioController().Play(attributes, false);
    }

    internal SkillResponse HelpWhilePlaying(AudioSessionAttributes attributes)
    {
      return Speech.GetAskResponse(
        "You are listening to Dopey Podcast. "+
        "You can say, Next or Previous to navigate through the episodes."+
        "At any time, you can say Pause to pause the audio and Resume to resume.");
    }

    internal SkillResponse UnhandledWhilePlaying(AudioSessionAttributes attributes)
    {
      return Speech.GetAskResponse(
        "Sorry, I could not understand. You can say, Next or Previous to navigate through the episodes.");
    }

    internal SkillResponse Help(AudioSessionAttributes attributes) => Speech.GetAskResponse("Welcome"); // TODO: extract message constants, same as launch string

    internal SkillResponse StopCancel(AudioSessionAttributes attributes) => Speech.GetTellResponse("Thanks for listening.", true);

    internal SkillResponse SessionEnded(AudioSessionAttributes attributes) => ResponseBuilder.Empty();

    internal SkillResponse Unhandled(AudioSessionAttributes attributes) => Speech.GetAskResponse("Sorry, I could not understand. Please say, play the audio, to begin the audio.");

    private AudioController GetAudioController()
    {
      var feedUrl = Environment.GetEnvironmentVariable("feed");
      var feedItems = FeedParser.ParseRss(feedUrl);

      _logger.LogLine($"Got {feedItems.Count} items from {feedUrl}.");

      return new AudioController(feedItems, _logger);
    }

    private int GetIndexFromToken(AudioSessionAttributes attributes, string token)
    {
      return attributes.PlayOrder.IndexOf(Int32.Parse(token));
    }
  }

  public static class Speech
  {
    public static SkillResponse GetTellResponse(string message, bool shouldEndSession)
    {
      var response = ResponseBuilder.Tell(new PlainTextOutputSpeech { Text = message });

      response.Response.ShouldEndSession = shouldEndSession;

      return response;
    }

    public static SkillResponse GetAskResponse(string message, string reprompt = null)
      => ResponseBuilder.Ask(
        new PlainTextOutputSpeech { Text = message },
        new Reprompt { OutputSpeech = new PlainTextOutputSpeech { Text = reprompt ?? message } });

    public static void AddOutputSpeech(SkillResponse response, string message)
      => response.Response.OutputSpeech = new PlainTextOutputSpeech { Text = message };

    public static SkillResponse GetContinueResponse()
      => new SkillResponse { Response = new ResponseBody { ShouldEndSession = false }};
  }
}