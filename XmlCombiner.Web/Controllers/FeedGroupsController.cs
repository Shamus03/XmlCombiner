using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XmlCombiner.Web.Controllers.Requests;
using XmlCombiner.Web.Domain;
using XmlCombiner.Web.Infrastructure;

namespace XmlCombiner.Web.Controllers
{
    [Route("api/[controller]")]
    public class FeedGroupsController : Controller
    {
        private readonly IFeedGroupRepository FeedGroupRepository;

        public FeedGroupsController(IFeedGroupRepository feedGroupRepository)
        {
            FeedGroupRepository = feedGroupRepository;
        }

        [HttpGet]
        [Produces(typeof(FeedGroup[]))]
        public async Task<IActionResult> GetFeedGroups()
        {
            return Ok(await FeedGroupRepository.GetFeedGroupsAsync());
        }


        [HttpGet("{id}")]
        [Produces(typeof(FeedGroup))]
        public async Task<IActionResult> GetFeedGroup([FromRoute] string id)
        {
            return Ok(await FeedGroupRepository.GetFeedGroupAsync(id));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedGroup([FromRoute] string id)
        {
            if (await FeedGroupRepository.DeleteFeedGroupAsync(id))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/hide")]
        public async Task<IActionResult> HideFeedGroup([FromRoute] string id)
        {
            if (await FeedGroupRepository.SetFeedGroupHiddenAsync(id, true))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/unhide")]
        public async Task<IActionResult> UnhideFeedGroup([FromRoute] string id)
        {
            if (await FeedGroupRepository.SetFeedGroupHiddenAsync(id, false))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Produces(typeof(FeedGroup))]
        public async Task<IActionResult> PostFeedGroupAsync([FromBody] FeedGroupPostRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var feedGroup = new FeedGroup
            {
                Description = request.Description.Trim(),
                BaseUrl = request.BaseUrl.Trim()
            };

            await FeedGroupRepository.AddFeedGroupAsync(feedGroup);
            return Created($"api/feedgroups/{feedGroup.Id}", feedGroup);
        }

        [HttpPost("{feedGroupId}/feeds")]
        [Produces(typeof(Feed))]
        public async Task<IActionResult> PostFeedToFeedGroup([FromRoute] string feedGroupId, [FromBody] FeedPostRequest request)
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

            if (await FeedGroupRepository.AddFeedToGroupAsync(feedGroupId, feed))
            {
                return Created($"api/feeds/{feed.Id}", feed);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
