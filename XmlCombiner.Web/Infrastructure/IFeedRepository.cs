using XmlCombiner.Web.Domain;

namespace XmlCombiner.Web.Infrastructure
{
    public interface IFeedRepository
    {
        bool AddFeed(Feed feed);
        bool DeleteFeed(string id);
        Feed[] GetFeeds(bool deleted = false);
        Feed UndeleteFeed(string id);
    }
}