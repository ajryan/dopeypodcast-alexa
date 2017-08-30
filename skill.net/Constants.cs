namespace skill.net
{
  public static class Constants
  {
    public const string AppId = "";

    public const string DynamoTableName = "LongFormAudioSample";

    public static class States
    {
      public const string StartMode          = "_START_MODE";
      public const string PlayMode           = "_PLAY_MODE";
      public const string ResumeDecisionMode = "_RESUME_DECISION_MODE";
    }

    public static class Events
    {
      public const string NewSession               = "NewSession";
      public const string LaunchRequest            = "LaunchRequest";
      public const string IntentRequest            = "IntentRequest";
      public const string PlayAudio                = "PlayAudio";
      public const string SessionEndedRequest      = "SessionEndedRequest";
      public const string AudioPlayerPrefix        = "AudioPlayer";
      public const string PlaybackControllerPrefix = "PlaybackController";
      public const string ElementSelected          = "ElementSelected";
      public const string Unhandled                = "Unhandled";

      public const string PlayCommandIssued        = "PlayCommandIssued";
      public const string PauseCommandIssued       = "PauseCommandIssued";
      public const string NextCommandIssued        = "NextCommandIssued";
      public const string PreviousCommandIssued    = "PreviousCommandIssued";

      public static class Amazon
      {
        public const string HelpIntent      = "AMAZON.HelpIntent";
        public const string CancelIntent    = "AMAZON.CancelIntent";
        public const string NextIntent      = "AMAZON.NextIntent";
        public const string PreviousIntent  = "AMAZON.PreviousIntent";
        public const string PauseIntent     = "AMAZON.PauseIntent";
        public const string StopIntent      = "AMAZON.StopIntent";
        public const string ResumeIntent    = "AMAZON.ResumeIntent";
        public const string LoopOnIntent    = "AMAZON.LoopOnIntent";
        public const string LoopOffIntent   = "AMAZON.LoopOffIntent";
        public const string ShuffleOnIntent = "AMAZON.ShuffleOnIntent";
        public const string ShuffleOffInten = "AMAZON.ShuffleOffIntent";
        public const string StartOverIntent = "AMAZON.StartOverIntent";
        public const string YesIntent       = "AMAZON.YesIntent";
        public const string NoIntent        = "AMAZON.NoIntent";
      }

      public static class Audio
      {
        public const string PlaybackStarted = "PlaybackStarted";
        public const string PlaybackFinished = "PlaybackFinished";
        public const string PlaybackStopped = "PlaybackStopped";
        public const string PlaybackNearlyFinished = "PlaybackNearlyFinished";
        public const string PlaybackFailed = "PlaybackFailed";
      }
    }
  }
}