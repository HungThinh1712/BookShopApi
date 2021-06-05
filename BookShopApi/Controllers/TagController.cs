using BookShopApi.Models;
using BookShopApi.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly TagService _tagService;

        public TagsController(TagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<BookTag>>> GetAll()
        {
            var tags = await _tagService.GetAsync();

            return Ok(tags); ;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create(BookTag tag)
        {
            var createdTag = await _tagService.CreateAsync(tag);
            return Ok(createdTag);

        }
       
    }
}
