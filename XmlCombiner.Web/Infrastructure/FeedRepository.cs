using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using XmlCombiner.Web.Domain;

namespace XmlCombiner.Web.Infrastructure
{
    public class FeedRepository : IFeedRepository
    {
        private static ReaderWriterLock FileLock;

        private readonly string FilePath;

        static FeedRepository()
        {
            FileLock = new ReaderWriterLock();
        }

        public FeedRepository()
        {
            FilePath = Environment.GetEnvironmentVariable("FEEDS_JSON") ?? "feeds.json";
        }

        public Feed[] GetFeeds()
        {
            FileLock.AcquireReaderLock(1000);
            try
            {
                var feeds = LoadFeeds();
                return feeds.ToArray();
            }
            finally
            {
                FileLock.ReleaseReaderLock();
            }
        }

        public bool AddFeed(Feed feed)
        {
            FileLock.AcquireWriterLock(1000);
            try
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
            finally
            {
                FileLock.ReleaseWriterLock();
            }
        }

        public bool DeleteFeed(string id)
        {
            FileLock.AcquireWriterLock(1000);
            try
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
            finally
            {
                FileLock.ReleaseWriterLock();
            }
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
            catch (Exception e) when (e is JsonSerializationException || e is FileNotFoundException)
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
