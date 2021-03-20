using BookShopApi.Models;
using BookShopApi.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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
        public async Task<ActionResult<IEnumerable<Models.Author>>> GetAll()
        {
            var author = await _authorService.GetAsync();
            return Ok(author);
        }

        [HttpGet("Admin/[action]")]
        public async Task<ActionResult<IEnumerable<Models.Author>>> GetAllAuthor([FromQuery] string name, int page)
        {
            var authors = await _authorService.GetAllAuthorAsync(name);
            foreach (var author in authors.Entities)
            {
                author.CreateAt = Convert.ToDateTime(author.CreateAt).ToLocalTime().ToString("yyyy-MM-dd");
            }
            return Ok(authors);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<Models.Author>> Get(
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
        public async Task<IActionResult> Create(Models.Author author)
        {
            var createdAuthor = await _authorService.CreateAsync(author);
            return Ok(createdAuthor);
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Update(JObject updatedAuthor)
        {
            var author = updatedAuthor["updatedAuthor"];
            string name = author["name"].ToString();
            string id = author["id"].ToString();
            await _authorService.UpdateAsync(id, name);
            return Ok(updatedAuthor);
        }
    }
}