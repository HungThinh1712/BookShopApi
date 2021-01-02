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
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorService _authorService;

        public AuthorsController(AuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAll()
        {
            var author = await _authorService.GetAsync();
            return Ok(author);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<Author>> Get(
            [FromQuery(Name = "id")] string id
            )
        {
            var author = await _authorService.GetAsync(id);
            if (author == null)
            {
                return BadRequest("author not found");
            }
            return Ok(author);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Create(Author author)
        {
            var createdAuthor = await _authorService.CreateAsync(author);
            return Ok(createdAuthor);

        }
        [HttpPut("[action]")]
        public async Task<IActionResult> Update(string id, Author updatedAuthor)
        {
            await _authorService.UpdateAsync(id, updatedAuthor);
            return Ok(updatedAuthor);
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string id)
        {
            var author = await _authorService.GetAsync(id);
            author.DeleteAt = DateTime.UtcNow;
            await _authorService.UpdateAsync(id, author);
            return Ok("Delete sucessfully");
        }
    }
}