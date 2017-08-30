using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using skill.net.PodcastFeed;

namespace skill.net.tests
{
    [TestClass]
    public class FeedParserTests
    {
        [TestMethod]
        public void ParseRssFeedTest()
        {
            var feedItems = FeedParser.ParseRss("http://dopeypodcast.podbean.com/feed/");
            Assert.IsTrue(feedItems.Count > 0);
            Assert.IsTrue(feedItems.OrderByDescending(i => i.PublishDate).First().Mp3Url.EndsWith("mp3"));
        }
    }
}
