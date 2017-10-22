using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using XmlCombiner.Web.Domain;

namespace XmlCombiner.Web.Infrastructure
{
    public class FeedGroupRepository : IFeedGroupRepository
    {
        private readonly XmlCombinerContext Context;

        public FeedGroupRepository(XmlCombinerContext context)
        {
            Context = context;
        }

        public async Task<bool> AddFeedToGroupAsync(string feedGroupId, Feed feed)
        {
            var feedGroup = await Context.FeedGroups.FindAsync(feedGroupId);

            if (feedGroup is null)
            {
                return false;
            }

            feedGroup.Feeds.Add(feed);
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<FeedGroup[]> GetFeedGroupsAsync()
        {
            return await (from feedGroup in Context.FeedGroups
                          select feedGroup).ToArrayAsync();
        }

        public async Task<bool> DeleteFeedGroupAsync(string id)
        {
            var feedGroup = await Context.FeedGroups.FindAsync(id);

            if (feedGroup is null)
            {
                return false;
            }

            Context.FeedGroups.Remove(feedGroup);
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<FeedGroup> GetFeedGroupAsync(string id)
        {
            return await (from feedGroup in Context.FeedGroups
                                                   .Include(g => g.Feeds)
                                                   .ThenInclude(f => f.AdditionalParameters)
                          where feedGroup.Id == id
                          select feedGroup).FirstOrDefaultAsync();
        }

        public async Task<bool> SetFeedGroupHiddenAsync(string id, bool hidden)
        {
            var feedGroup = await Context.FeedGroups.FindAsync(id);

            if (feedGroup is null)
            {
                return false;
            }

            feedGroup.Hidden = hidden;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task AddFeedGroupAsync(FeedGroup feedGroup)
        {
            Context.FeedGroups.Add(feedGroup);
            await Context.SaveChangesAsync();
        }
    }
}
