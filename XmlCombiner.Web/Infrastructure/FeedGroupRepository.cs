using Microsoft.EntityFrameworkCore;
using System;
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
            var feedGroup = await Context.FeedGroups
                .FindAsync(feedGroupId);

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
            return await Context.FeedGroups
                .ToArrayAsync();
        }

        public Task<bool> DeleteFeedGroupAsync(string id)
        {
            return ActionOnFeedGroupAsync(id, fg => Context.FeedGroups.Remove(fg));
        }

        public async Task<FeedGroup> GetFeedGroupAsync(string id)
        {
            return await Context.FeedGroups
                .Where(g => g.Id == id)
                .Include(g => g.Feeds)
                .ThenInclude(f => f.AdditionalParameters)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SetFeedGroupHiddenAsync(string id, bool hidden)
        {
            var feedGroup = await Context.FeedGroups
                .FindAsync(id);

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

        public Task<bool> UpdateFeedGroupDescriptionAsync(string id, string description)
        {
            return ActionOnFeedGroupAsync(id, fg => fg.Description = description);
        }

        private async Task<bool> ActionOnFeedGroupAsync(string id, Action<FeedGroup> action)
        {
            var feedGroup = await Context.FeedGroups
               .FindAsync(id);

            if (feedGroup is null)
            {
                return false;
            }

            action(feedGroup);

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
