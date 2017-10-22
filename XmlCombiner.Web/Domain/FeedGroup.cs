using System.Collections.Generic;

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
    }
}
