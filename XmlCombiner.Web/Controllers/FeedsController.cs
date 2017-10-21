using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using XmlCombiner.Web.Controllers.Requests;
using XmlCombiner.Web.Domain;
using XmlCombiner.Web.Infrastructure;

namespace XmlCombiner.Web.Controllers
{
    [Route("api/[controller]")]
    public class FeedsController : Controller
    {
        private readonly XmlCombinerContext Context;

        public FeedsController(XmlCombinerContext context)
        {
            Context = context;
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

            var feeds = Context.Feeds.Include(f => f.AdditionalParameters).Where(f => f.Hidden == false);
            foreach (Feed feed in feeds)
            {
                var feedDocument = XDocument.Load(feed.RssPageUrl);
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
            return Ok(Context.Feeds.Include(f => f.AdditionalParameters).Where(f => f.Hidden == false));
        }

        [HttpGet("hidden")]
        [Produces(typeof(Feed[]))]
        public IActionResult GetHiddenFeeds()
        {
            return Ok(Context.Feeds.Include(f => f.AdditionalParameters).Where(f => f.Hidden == true));
        }

        [HttpPost]
        [Produces(typeof(Feed))]
        public async Task<IActionResult> PostFeed([FromBody] FeedPostRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var feed = new Feed
            {
                Name = request.Name.Trim(),
                BaseUrl = request.BaseUrl.Trim(),
                AdditionalParameters = request.AdditionalParameters.Select(p =>
                    new AdditionalParameter
                    {
                        Parameter = p.Parameter.Trim()
                    }
                ).ToList()
            };

            try
            {
                XDocument.Load(feed.RssPageUrl);
            }
            catch (Exception e) when (e is IOException || e is WebException)
            {
                return BadRequest("Feed could not be loaded");
            }
            catch (XmlException)
            {
                return BadRequest("Feed url does not parse to XML");
            }

            Context.Feeds.Add(feed);
            await Context.SaveChangesAsync();

            return Created($"api/feed/{feed.Id}", feed);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeed([FromRoute] string id)
        {
            var feed = await Context.Feeds.FindAsync(id);
            if (feed != null)
            {
                Context.Feeds.Remove(feed);
                await Context.SaveChangesAsync();
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/hide")]
        public async Task<IActionResult> HideFeed([FromRoute] string id)
        {
            return await SetFeedHidden(id, true);
        }

        [HttpPost("{id}/unhide")]
        public async Task<IActionResult> UnhideFeed([FromRoute] string id)
        {
            return await SetFeedHidden(id, false);
        }

        private async Task<IActionResult> SetFeedHidden(string id, bool hidden)
        {
            var feed = await Context.Feeds.Include(f => f.AdditionalParameters).FirstOrDefaultAsync(f => f.Id == id);
            if (feed != null)
            {
                feed.Hidden = hidden;
                await Context.SaveChangesAsync();
                return Ok(feed);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
