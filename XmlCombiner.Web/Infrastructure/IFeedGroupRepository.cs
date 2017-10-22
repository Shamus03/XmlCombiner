using System.Threading.Tasks;
using XmlCombiner.Web.Domain;

namespace XmlCombiner.Web.Infrastructure
{
    public interface IFeedGroupRepository
    {
        Task<bool> AddFeedToGroupAsync(string feedGroupId, Feed feed);
        Task<bool> DeleteFeedGroupAsync(string id);
        Task<FeedGroup[]> GetFeedGroupsAsync();
        Task<FeedGroup> GetFeedGroupAsync(string id);
        Task<bool> SetFeedGroupHiddenAsync(string id, bool hidden);
        Task AddFeedGroupAsync(FeedGroup feedGroup);
    }
}