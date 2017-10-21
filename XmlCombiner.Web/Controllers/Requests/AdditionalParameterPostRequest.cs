using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace XmlCombiner.Web.Controllers.Requests
{
    public class AdditionalParameterPostRequest
    {
        [Required]
        public string Parameter { get; set; }
    }
}
