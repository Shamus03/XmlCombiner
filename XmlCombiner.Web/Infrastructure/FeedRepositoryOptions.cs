namespace XmlCombiner.Web.Infrastructure
{
    public class FeedRepositoryOptions
    {
        public string FilePath { get; set; }

        public FeedRepositoryOptions()
        {
            FilePath = "feeds.json";
        }
    }
}
