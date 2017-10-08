using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace XmlCombiner.Web.Controllers
{
    [Route("api/[controller]")]
    public class RssController : Controller
    {
        private readonly string FeedsFilename = Environment.GetEnvironmentVariable("FEEDS_JSON") ?? "feeds.json";

        [HttpGet("xml")]
        public FileResult GetCombinedFeedsXml()
        {
            XDocument combinedDocument = CreateBaseDocument("Combined RSS feed");
            XElement channel = combinedDocument.Element("rss").Element("channel");

            var feeds = LoadFeeds();
            foreach (string feedUrl in feeds.Values)
            {
                var feedDocument = XDocument.Load(feedUrl);
                var items = feedDocument.Root.Element("channel").Elements("item");
                channel.Add(items);
            }

            byte[] fileContent = Encoding.UTF8.GetBytes(combinedDocument.ToString());
            return File(fileContent, "application/xml");
        }

        [HttpGet]
        public Dictionary<string, string> GetFeeds()
        {
            return LoadFeeds();
        }

        [HttpPost]
        public IActionResult AddFeed([FromQuery] string feedUrl)
        {
            if (feedUrl == null)
            {
                return BadRequest();
            }

            try
            {
                XDocument.Load(feedUrl);
            }
            catch (XmlException)
            {
                return StatusCode((int)HttpStatusCode.Conflict, "Feed url does not parse to XML");
            }

            Dictionary<string, string> feeds = LoadFeeds();
            string id = Guid.NewGuid().ToString();
            feeds[id] = feedUrl;
            SaveFeeds(feeds);
            return Created($"api/rss/{id}", id);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFeed([FromRoute] string id)
        {
            Dictionary<string, string> feeds = LoadFeeds();
            if (feeds.Remove("id"))
            {
                SaveFeeds(feeds);
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetFeed([FromRoute] string id)
        {
            Dictionary<string, string> feeds = LoadFeeds();
            if (feeds.TryGetValue(id, out string feedUrl))
            {
                return Ok(feedUrl);
            }
            else
            {
                return NotFound();
            }
        }

        private Dictionary<string, string> LoadFeeds()
        {
            Dictionary<string, string> feeds;

            try
            {
                using (StreamReader r = new StreamReader(FeedsFilename))
                {
                    string json = r.ReadToEnd();
                    feeds = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
            }
            catch (FileNotFoundException)
            {
                feeds = null;
            }

            if (feeds == null)
            {
                feeds = new Dictionary<string, string>();
            }

            return feeds;
        }

        private void SaveFeeds(Dictionary<string, string> feeds)
        {
            using (StreamWriter w = new StreamWriter(FeedsFilename, false))
            {
                w.WriteLine(JsonConvert.SerializeObject(feeds));
            }
        }

        private static XDocument CreateBaseDocument(string title)
        {
            var document = new XDocument();
            var rss = new XElement("rss");
            document.Add(rss);

            var channel = new XElement("channel");
            rss.Add(channel);

            var channelTitle = new XElement("title")
            {
                Value = title
            };
            channel.Add(channelTitle);

            return document;
        }
    }
}
