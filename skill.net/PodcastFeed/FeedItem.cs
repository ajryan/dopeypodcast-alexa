using System;

namespace skill.net.PodcastFeed
{
  public class FeedItem
  {
    public string Link { get; internal set; }
    public string Title { get; internal set; }
    public string Description { get; internal set; }
    public DateTime PublishDate { get; internal set; }
    public string Mp3Url { get; internal set; }
    public string ArtUrl { get; internal set; }
  }
}
