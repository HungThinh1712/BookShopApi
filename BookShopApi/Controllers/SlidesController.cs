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
    public class SlidesController : ControllerBase
    {
        private readonly SlideService _slideService;

        public SlidesController(SlideService slideService)
        {
            _slideService = slideService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Slide>>> GetAll()
        {
            var slide = await _slideService.GetAsync();
            return Ok(slide);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<Slide>> Get(
            [FromQuery(Name = "id")] string id
            )
        {
            var slide = await _slideService.GetAsync(id);
            if (slide == null)
            {
                return BadRequest("slide not found");
            }
            return Ok(slide);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Create(Slide slide)
        {
            var createdSlide = await _slideService.CreateAsync(slide);
            return Ok(createdSlide);

        }
        [HttpPut("[action]")]
        public async Task<IActionResult> Update(string id, Slide updatedSlide)
        {
            await _slideService.UpdateAsync(id, updatedSlide);
            return Ok(updatedSlide);
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string id)
        {
            var slide = await _slideService.GetAsync(id);
            slide.IsDeleted = true;
            await _slideService.UpdateAsync(id, slide);
            return Ok("Delete sucessfully");
        }
    }
}