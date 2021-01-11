using BookShopApi.Models;
using BookShopApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly TagService _tagservice;

        public TagsController(TagService tagservice)
        {
            _tagservice = tagservice;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAll()
        {
            var tags = await _tagservice.GetAsync();
            return Ok(tags);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<Tag>> Get(string id)
        {
            var tag = await _tagservice.GetAsync(id);
            if (tag == null)
            {
                return BadRequest("Tag not found");
            }
            return Ok(tag);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Create(Tag tag)
        {           
              
            await _tagservice.CreateAsync(tag);
            return Ok(tag);
        }
        [HttpPut("[action]")]
        public async Task<IActionResult> Update(string id, Tag updatedTag)
        {
       
            await _tagservice.UpdateAsync(id, updatedTag);
            return Ok(updatedTag);
        }
        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string id)
        {
            
            var tag = await _tagservice.GetAsync(id);
            if (tag == null)
            {
                return BadRequest("Not found");
            }
            await _tagservice.RemoveAsync(id);
            return Ok("Delete sucessfully");
        }
    }
}
