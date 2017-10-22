using System.ComponentModel.DataAnnotations;

namespace XmlCombiner.Web.Controllers.Requests
{
    public class FeedGroupPostRequest
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public string BaseUrl { get; set; }
    }
}
