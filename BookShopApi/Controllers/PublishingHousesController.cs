using BookShopApi.Models;
using BookShopApi.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishingHousesController : ControllerBase
    {
        private readonly PublishingHouseService _publishingHouseService;

        public PublishingHousesController(PublishingHouseService publishingHouseService)
        {
            _publishingHouseService = publishingHouseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PublishingHouse>>> GetAll()
        {
            var publishingHouse = await _publishingHouseService.GetAsync();
            return Ok(publishingHouse);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<PublishingHouse>> Get(
            [FromQuery(Name = "id")] string id
            )
        {
            var publishingHouse = await _publishingHouseService.GetAsync(id);
            if (publishingHouse == null)
            {
                return BadRequest("publishingHouse not found");
            }
            return Ok(publishingHouse);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Create(PublishingHouse publishingHouse)
        {
            var createdPublishingHouse = await _publishingHouseService.CreateAsync(publishingHouse);
            return Ok(createdPublishingHouse);

        }
        [HttpPut("[action]")]
        public async Task<IActionResult> Update(string id, PublishingHouse updatedPublishingHouse)
        {
            await _publishingHouseService.UpdateAsync(id, updatedPublishingHouse);
            return Ok(updatedPublishingHouse);
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string id)
        {
            var publishingHouse = await _publishingHouseService.GetAsync(id);
            publishingHouse.DeleteAt = DateTime.UtcNow;
            await _publishingHouseService.UpdateAsync(id, publishingHouse);
            return Ok("Delete sucessfully");
        }
    }
}