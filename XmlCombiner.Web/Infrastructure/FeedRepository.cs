using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using XmlCombiner.Web.Domain;

namespace XmlCombiner.Web.Infrastructure
{
    public class FeedRepository : IFeedRepository
    {
        private readonly XmlCombinerContext Context;

        public FeedRepository(XmlCombinerContext context)
        {
            Context = context;
        }

        public async Task<bool> DeleteFeedAsync(string id)
        {
            var feed = await Context.Feeds.FindAsync(id);

            if (feed is null)
            {
                return false;
            }

            Context.Feeds.Remove(feed);
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Feed[]> GetAllActiveFeedsAsync()
        {
            return await (from feedGroup in Context.FeedGroups
                                                   .Include(g => g.Feeds)
                                                   .ThenInclude(f => f.AdditionalParameters)
                          where !feedGroup.Hidden
                          from feed in feedGroup.Feeds
                          select feed).ToArrayAsync();
        }
    }
}
