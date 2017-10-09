using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace XmlCombiner.Web.Domain
{
    public class Feed
    {
        public string Id { get; set; }

        [Required]
        public string BaseUrl { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<string> AdditionalParameters { get; }

        public string EncodedFeedUrl => BaseUrl + HttpUtility.UrlEncode($"{string.Join(" ", AdditionalParameters)} {Name}");

        public Feed(string baseUrl, string name)
        {
            this.BaseUrl = baseUrl;
            this.Name = name;
            this.AdditionalParameters = new HashSet<string>();
        }
    }
}
