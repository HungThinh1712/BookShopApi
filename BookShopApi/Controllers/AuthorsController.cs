using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Authors;
using BookShopApi.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorService _authorService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AuthorsController(AuthorService authorService, IWebHostEnvironment hostEnvironment)
        {
            _authorService = authorService;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<EntityList<AuthorsInAdminViewModel>>> GetAll(
            [FromQuery] string name,
            [FromQuery] int page,
            [FromQuery] int pageSize)
        {
            

            var authors = await _authorService.GetAsync(name,page,pageSize,Request);

                return Ok(authors);
        }


        [HttpGet("[action]")]
        public async Task<ActionResult<Models.Author>> Get(
            [FromQuery(Name = "id")] string id
            )
        {
            var author = await _authorService.GetAsync(id,Request);
            if (author == null)
            {
                return BadRequest("author not found");
            }
            return Ok(author);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromForm]Models.Author author)
        {
            
            var createdAuthor = await _authorService.CreateAsync(author, Request);
            return Ok(createdAuthor);

        }
        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string id)
        {

            await _authorService.RemoveAsync(id);
            return Ok();

        }
        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromForm] UpdatedAuthor updatedAuthor)
        {
            var author = await _authorService.GetAsync(updatedAuthor.Id);
            author.Description = updatedAuthor.Description;
            author.BirthDay = updatedAuthor.BirthDay;
            author.Name = updatedAuthor.Name;
            author.ImgUrl = updatedAuthor.ImgUrl;

           

            var result = await _authorService.UpdateAsync(author);
            var authorRM = new AuthorsInAdminViewModel
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                BirthDay = result.BirthDay.ToLocalTime().ToString("yyyy-MM-dd"),
                ImgUrl = result.ImgUrl,
            };
            return Ok(authorRM);
        }
       
    }
}