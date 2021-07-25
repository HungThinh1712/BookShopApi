using BookShopApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BookShopApi.Service;
using System;
using BookShopApi.Models.ViewModels.BookTypes;
using Newtonsoft.Json.Linq;

namespace BookShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypesController : ControllerBase
    {
        private readonly TypeService _typeService;

        public TypesController(TypeService typeService)
        {
            _typeService = typeService;
        }

        [HttpGet("[action]")]    
        public async Task<ActionResult<EntityList<BookTypeViewModel>>> GetAll(
            [FromQuery] string name,
            [FromQuery] int page,
            [FromQuery] int pageSize)
        {
            var types = await _typeService.GetAsync(name,page,pageSize);
           
            return Ok(types); ;
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<EntityList<BookTypeViewModel>>> GetAllIncludesDeleted(
            [FromQuery] string name,
            [FromQuery] int page,
            [FromQuery] int pageSize)
        {
            var types = await _typeService.GetIncludeDeleteAsync(name, page, pageSize);

            return Ok(types); ;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<System.Type>> Get(
            [FromQuery(Name = "id")] string id
            )
        {
            var type = await _typeService.GetAsync(id);
            if (type == null)
            {
                return BadRequest("type not found");
            }
            return Ok(type);
        }

        [HttpPost("[action]")]    
        public async Task<IActionResult> Create(BookType type)
        {
           var createdType = await _typeService.CreateAsync(type);
            return Ok(createdType);

        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Update(BookType type)
        {
            var result = await _typeService.UpdateAsync(type.Id,type);

            return Ok(new BookTypeViewModel
            {
                Id = result.Id,
                Name = result.Name,
                CreateAt = result.CreateAt.ToLocalTime().ToString("yyyy-MM-dd"),
            }) ;
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string id)
        {
           
            
            await _typeService.RemoveAsync(id);
            return Ok();
        }

        [HttpGet("Admin/[action]")]
        public async Task<ActionResult<IEnumerable<Models.BookType>>> GetAllType([FromQuery] string name, int page)
        {
            var types = await _typeService.GetAllTypeAsync(name);
            return Ok(types);
        }
    }
}