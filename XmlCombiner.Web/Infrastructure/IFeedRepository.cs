using System.Threading.Tasks;
using XmlCombiner.Web.Domain;

namespace XmlCombiner.Web.Infrastructure
{
    public interface IFeedRepository
    {
        Task<bool> DeleteFeedAsync(string id);
        Task<Feed[]> GetAllActiveFeedsAsync();
    }
}