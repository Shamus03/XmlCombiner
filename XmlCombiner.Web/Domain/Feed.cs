using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XmlCombiner.Web.Domain
{
    public class Feed
    {
        public string Id { get; set; }

        public string BaseUrl { get; set; }

        public string Name { get; set; }

        public ICollection<AdditionalParameter> AdditionalParameters { get; set; }

        public string SearchPageUrl => BaseUrl + "&q=" + HttpUtility.UrlEncode($"{string.Join(" ", AdditionalParameters.Select(p => p.Parameter).Concat(new[] { Name }))}");

        public string RssPageUrl => SearchPageUrl + "&page=rss";

        public Feed()
        {
            AdditionalParameters = new HashSet<AdditionalParameter>();
        }
    }
}
