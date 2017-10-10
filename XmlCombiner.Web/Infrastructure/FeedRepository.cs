using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XmlCombiner.Web.Domain;

namespace XmlCombiner.Web.Infrastructure
{
    public class FeedRepository : IFeedRepository
    {
        private readonly string FilePath;

        public FeedRepository()
        {
            FilePath = Environment.GetEnvironmentVariable("FEEDS_JSON") ?? "feeds.json";
        }

        public Feed[] GetFeeds()
        {
            return LoadFeeds().ToArray();
        }

        public bool AddFeed(Feed feed)
        {
            ICollection<Feed> feeds = LoadFeeds();
            Feed found = feeds.FirstOrDefault(f => f.Id == feed.Id);

            if (found != null)
            {
                return false;
            }

            feeds.Add(feed);
            SaveFeeds(feeds);
            return true;
        }

        public bool DeleteFeed(string id)
        {
            ICollection<Feed> feeds = LoadFeeds();
            Feed found = feeds.FirstOrDefault(f => f.Id == id);

            if (found == null)
            {
                return false;
            }

            feeds.Remove(found);
            SaveFeeds(feeds);
            return true;
        }

        private ICollection<Feed> LoadFeeds()
        {
            ICollection<Feed> feeds;
            try
            {
                using (StreamReader r = new StreamReader(FilePath))
                {
                    string json = r.ReadToEnd();
                    feeds = JsonConvert.DeserializeObject<ICollection<Feed>>(json);
                }
            }
            catch (JsonSerializationException)
            {
                feeds = null;
            }
            catch (FileNotFoundException)
            {
                feeds = null;
            }

            if (feeds == null)
            {
                feeds = new HashSet<Feed>();
            }

            return feeds;
        }

        private void SaveFeeds(ICollection<Feed> feeds)
        {
            using (StreamWriter w = new StreamWriter(FilePath, false))
            {
                w.WriteLine(JsonConvert.SerializeObject(feeds));
            }
        }
    }
}
