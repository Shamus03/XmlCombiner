using System;
using System.Collections.Generic;
using System.Linq;

namespace XmlCombiner.Web.Domain
{
    public class FeedGroup 
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public string BaseUrl { get; set; }

        public bool Hidden { get; set; }

        public ICollection<Feed> Feeds { get; set; }

        public FeedGroup()
        {
            Feeds = new HashSet<Feed>();
        }

        public FeedGroup Copy()
        {
            return new FeedGroup
            {
                Description = Description,
                BaseUrl = BaseUrl,
                Hidden = Hidden,
                Feeds = Feeds.Select(f => f.Copy()).ToArray(),
            };
        }
    }
}
