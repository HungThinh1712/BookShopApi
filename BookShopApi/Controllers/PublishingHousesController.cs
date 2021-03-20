using BookShopApi.Models;
using BookShopApi.Service;
using Microsoft.AspNetCore.Mvc;
using BookShopApi.Models.ViewModels.PublishingHouses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Mapster;

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
        public async Task<ActionResult<IEnumerable<Models.PublishingHouse>>> GetAll()
        {
            var publishingHouse = await _publishingHouseService.GetAsync();
            return Ok(publishingHouse);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<Models.PublishingHouse>> Get(
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
        public async Task<IActionResult> Create(Models.PublishingHouse publishingHouse)
        {
            var createdPublishingHouse = await _publishingHouseService.CreateAsync(publishingHouse);
            return Ok(createdPublishingHouse);

        }

        [HttpPut("[action]")]   
        public async Task<IActionResult> Update(JObject updatedPublishHouse)
        {
            var publishHouse = updatedPublishHouse["updatedPublishHouse"];
            string name = publishHouse["name"].ToString();
            string id = publishHouse["id"].ToString();
            await _publishingHouseService.UpdateAsync(id, name);
            return Ok(updatedPublishHouse);
        }

        [HttpGet("Admin/[action]")]
        public async Task<ActionResult<IEnumerable<Models.PublishingHouse>>> GetAllPublishingHouse([FromQuery] string name, int page)
        {
            var publishingHouses = await _publishingHouseService.GetAllTypeAsync(name);
            foreach (var publishingHouse in publishingHouses.Entities)
            {
                publishingHouse.CreateAt = Convert.ToDateTime(publishingHouse.CreateAt).ToLocalTime().ToString("yyyy-MM-dd");
            }
            return Ok(publishingHouses);
        }
    }
}