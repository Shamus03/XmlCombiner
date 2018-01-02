using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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

            if (await FeedGroupRepository.AddFeedToGroupAsync(feedGroupId, feed))
            {
                return Created($"api/feeds/{feed.Id}", feed);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPatch("{feedGroupId}")]
        public async Task<IActionResult> UpdateFeedGroupDescription([FromRoute] string feedGroupId, [FromBody] JObject body)
        {
            string updatedDescription = null;
            string updatedBaseUrl = null;

            if (body.TryGetValue("description", out JToken descriptionToken))
            {
                if (descriptionToken.Type != JTokenType.String)
                {
                    ModelState.AddModelError("description", "description must be a string");
                }
                else
                {
                    updatedDescription = descriptionToken.Value<string>();
                }
            }

            if (body.TryGetValue("baseUrl", out JToken baseUrlToken))
            {
                if (baseUrlToken.Type != JTokenType.String)
                {
                    ModelState.AddModelError("baseUrl", "baseUrl must be a string");
                }
                else
                {
                    updatedBaseUrl = baseUrlToken.Value<string>();
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool updated = await FeedGroupRepository.UpdateFeedGroupAsync(feedGroupId, fg =>
            {
                if (updatedDescription != null)
                {
                    fg.Description = updatedDescription;
                }

                if (updatedBaseUrl != null)
                {
                    fg.BaseUrl = updatedBaseUrl;
                }
            });

            if (!updated)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("{feedGroupId}/copy")]
        [Produces(typeof(FeedGroup))]
        public async Task<IActionResult> CopyFeedGroup([FromRoute] string feedGroupId)
        {
            var original = await FeedGroupRepository.GetFeedGroupAsync(feedGroupId);

            if (original == null)
            {
                return NotFound();
            }

            var copy = original.Copy();

            copy.Description += " (copy)";

            await FeedGroupRepository.AddFeedGroupAsync(copy);

            return CreatedAtAction(nameof(GetFeedGroup), new { id = copy.Id }, copy);
        }
    }
}
