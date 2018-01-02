using System;

namespace XmlCombiner.Web.Domain
{
    public class AdditionalParameter
    {
        public string Id { get; set; }

        public string Parameter { get; set; }

        public AdditionalParameter()
        {
        }

        public AdditionalParameter Copy()
        {
            return new AdditionalParameter
            {
                Parameter = Parameter,
            };
        }
    }
}
