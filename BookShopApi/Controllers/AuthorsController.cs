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
            author.ImageName = await SaveImageAsync(author.ImageFile);
            var createdAuthor = await _authorService.CreateAsync(author, Request);
            return Ok(createdAuthor);

        }
        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromForm] UpdatedAuthor updatedAuthor)
        {
            var author = await _authorService.GetAsync(updatedAuthor.Id);
            author.Description = updatedAuthor.Description;
            author.BirthDay = updatedAuthor.BirthDay;
            author.Name = updatedAuthor.Name;

            if (updatedAuthor.ImageFile != null)
            {
                DeleteImage(author.ImageName);
                author.ImageName = await SaveImageAsync(updatedAuthor.ImageFile);
            }

            await _authorService.UpdateAsync(author);
            return Ok(updatedAuthor);
        }
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string imageName = new string(Path.GetFileNameWithoutExtension(imageFile.FileName).Take(10).ToArray()).Replace(' ', '-');
            imageName = imageName + DateTime.Now.ToString("yymmssff") + Path.GetExtension(imageFile.FileName);
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return imageName;
        }
        private void DeleteImage(string imageName)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }
    }
}