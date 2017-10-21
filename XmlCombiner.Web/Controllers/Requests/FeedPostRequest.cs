using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XmlCombiner.Web.Domain;

namespace XmlCombiner.Web.Controllers.Requests
{
    public class FeedPostRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string BaseUrl { get; set; }

        public List<AdditionalParameterPostRequest> AdditionalParameters { get; set; }
    }
}
