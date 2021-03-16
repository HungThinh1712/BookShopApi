using BookShopApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BookShopApi.Service;
using System;
using BookShopApi.Models.ViewModels.BookTypes;

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
        public async Task<ActionResult<List<BookTypeViewModel>>> GetAll()
        {
            var types = await _typeService.GetAsync();
            return Ok(types);
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
        public async Task<IActionResult> Update(string id, BookType updatedType)
        {
      
            await _typeService.UpdateAsync(id, updatedType);
            return Ok(updatedType);
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string id)
        {
           
            var type = await _typeService.GetAsync(id);
          
            type.DeleteAt = DateTime.UtcNow;
            await _typeService.UpdateAsync(id,type);
            return Ok("Delete sucessfully");
        }

        [HttpGet("Admin/[action]")]
        public async Task<ActionResult<IEnumerable<Models.BookType>>> GetAllType([FromQuery] string name, int page)
        {
            var types = await _typeService.GetAllTypeAsync(name);
            foreach (var type in types.Entities)
            {
                type.CreateAt = Convert.ToDateTime(type.CreateAt).ToLocalTime().ToString("yyyy-MM-dd");
            }
            return Ok(types);
        }
    }
}