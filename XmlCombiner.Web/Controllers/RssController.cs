using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace XmlCombiner.Web.Controllers
{
    [Route("api/[controller]")]
    public class RssController : Controller
    {
        private const string FEEDS_FILE = "feeds.json";

        [HttpGet("xml")]
        public FileResult GetCombinedFeedsXml()
        {
            XDocument combinedDocument = CreateBaseDocument("Shamus' RSS feed");
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
                using (StreamReader r = new StreamReader(FEEDS_FILE))
                {
                    string json = r.ReadToEnd();
                    feeds = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
            }
            catch (FileNotFoundException)
            {
                feeds = new Dictionary<string, string>();
            }

            return feeds;
        }

        private void SaveFeeds(Dictionary<string, string> feeds)
        {
            using (StreamWriter w = new StreamWriter(FEEDS_FILE, false))
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
