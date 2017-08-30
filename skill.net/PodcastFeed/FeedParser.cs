using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

namespace skill.net.PodcastFeed
{
  public static class FeedParser
  {
    public static List<FeedItem> ParseRss(string url)
    {
      var client = new HttpClient();
      var feedContent = client.GetStringAsync(new Uri(url)).Result;

      XDocument doc = XDocument.Parse(feedContent);
      XNamespace itunes = "http://www.itunes.com/dtds/podcast-1.0.dtd";

      // RSS/Channel/item
      var entries = from item in
                        doc.Root.Descendants()
                          .First(i => i.Name.LocalName == "channel")
                          .Elements()
                          .Where(i => i.Name.LocalName == "item")
                    select new FeedItem
                    {
                      Title       = item.Element("title").Value,
                      Link        = item.Element("link").Value,
                      Description = item.Element("description").Value,
                      PublishDate = ParseDate(item.Element("pubDate").Value),
                      Mp3Url      = item.Element("enclosure").Attribute("url").Value,
                      ArtUrl      = item.Descendants(itunes + "image").FirstOrDefault()?.FirstAttribute?.Value,
                    };

      return entries.OrderByDescending(i => i.PublishDate).ToList();
    }

    private static DateTime ParseDate(string date)
    {
      return DateTime.TryParse(date, out var result) ? result : DateTime.MinValue;
    }
  }
}
