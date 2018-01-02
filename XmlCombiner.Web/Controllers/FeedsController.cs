using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XmlCombiner.Web.Domain;
using XmlCombiner.Web.Infrastructure;

namespace XmlCombiner.Web.Controllers
{
    [Route("api/[controller]")]
    public class FeedsController : Controller
    {
        private readonly IFeedRepository FeedRepository;

        public FeedsController(IFeedRepository feedRepository)
        {
            FeedRepository = feedRepository;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeed([FromRoute] string id)
        {
            if (await FeedRepository.DeleteFeedAsync(id))
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("xml")]
        public async Task<IActionResult> GetCombinedFeedsXml()
        {
            var document = new XDocument();
            var rss = new XElement("rss");
            document.Add(rss);

            var channel = new XElement("channel");
            rss.Add(channel);

            var channelTitle = new XElement("title")
            {
                Value = "Combined rss feed"
            };
            channel.Add(channelTitle);

            var feeds = await FeedRepository.GetAllActiveFeedsAsync();

            foreach (Feed feed in feeds)
            {
                try
                {
                    var feedDocument = XDocument.Load(feed.RssPageUrl);
                    var items = feedDocument.Root.Element("channel").Elements("item");
                    channel.Add(items);
                }
                catch (Exception e) when (e is IOException || e is WebException || e is XmlException)
                {
                    // ignore feeds that do not load or parse to expected xml
                }
            }

            byte[] fileContent = Encoding.UTF8.GetBytes(document.ToString());
            return File(fileContent, "application/xml");
        }
    }
}
