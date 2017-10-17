using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [HttpGet("xml")]
        public IActionResult GetCombinedFeedsXml()
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

            Feed[] feeds = FeedRepository.GetFeeds();
            foreach (Feed feed in feeds)
            {
                var feedDocument = XDocument.Load(feed.EncodedFeedUrl);
                var items = feedDocument.Root.Element("channel").Elements("item");
                channel.Add(items);
            }

            byte[] fileContent = Encoding.UTF8.GetBytes(document.ToString());
            return File(fileContent, "application/xml");
        }

        [HttpGet]
        [Produces(typeof(Feed[]))]
        public IActionResult GetFeeds()
        {
            return Ok(FeedRepository.GetFeeds());
        }

        [HttpGet("deleted")]
        [Produces(typeof(Feed[]))]
        public IActionResult GetDeletedFeeds()
        {
            return Ok(FeedRepository.GetFeeds(deleted: true));
        }

        [HttpPost]
        [Produces(typeof(Feed))]
        public IActionResult PostFeed([FromBody] Feed feed)
        {
            ModelState.Remove(nameof(Feed.Id));

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                XDocument.Load(feed.EncodedFeedUrl);
            }
            catch(Exception e) when (e is IOException || e is WebException)
            {
                return BadRequest("Feed could not be loaded");
            }
            catch (XmlException)
            {
                return BadRequest("Feed url does not parse to XML");
            }

            feed.Id = Guid.NewGuid().ToString();
            FeedRepository.AddFeed(feed);

            return Created($"api/rss/{feed.Id}", feed);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFeed([FromRoute] string id)
        {
            if (FeedRepository.DeleteFeed(id))
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/undelete")]
        public IActionResult UndeleteFeed([FromRoute] string id)
        {
            var feed = FeedRepository.UndeleteFeed(id);
            if (feed != null)
            {
                return Ok(feed);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
