using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Linq;
using Newtonsoft.Json;

namespace XmlCombiner.Web.Domain
{
    public class Feed
    {
        public string Id { get; set; }

        public string BaseUrl { get; set; }

        public string Name { get; set; }

        public bool Hidden { get; set; }

        public ICollection<AdditionalParameter> AdditionalParameters { get; set; }

        public string SearchPageUrl => BaseUrl + "&q=" + HttpUtility.UrlEncode($"{string.Join(" ", AdditionalParameters.Select(p => p.Parameter).Concat(new[] { Name }))}");

        public string RssPageUrl => SearchPageUrl + "&page=rss";

        public Feed()
        {
        }
    }
}
