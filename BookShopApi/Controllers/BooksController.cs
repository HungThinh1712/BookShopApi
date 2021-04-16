using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels;
using BookShopApi.Models.ViewModels.Books;
using BookShopApi.Service;
using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;
        private readonly TypeService _typeService;
        private readonly PublishingHouseService _publishingHouseService;
        private readonly AuthorService _authorService;
        private readonly IWebHostEnvironment _hostEnvironment;
        public BooksController(BookService bookService, 
                               TypeService typeService,
                               PublishingHouseService publishingHouseService,
                               AuthorService authorService,
                               IWebHostEnvironment hostEnvironment)
        {
            _bookService = bookService;
            _typeService = typeService;
            _publishingHouseService = publishingHouseService;
            _authorService = authorService;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<List<BooksViewModel>>> GetAll(
            [FromQuery] int index)
        {
            var books = await _bookService.GetAsync(index, Request);

            return Ok(books);
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<List<BooksViewModel>>> GetBookByZone(
            [FromQuery] int index ,
            [FromQuery] string zoneType)
        {
            var books = await _bookService.GetByZoneAsync(index, Request,zoneType);

            return Ok(books);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<BooksViewModel>>> GetBookByTag(
            [FromQuery] int index,
            [FromQuery] string tag)
        {
            var books = await _bookService.GetByTagAsync(index, Request, tag);

            return Ok(books);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<BooksViewModel>>> GetBookByTypeId(
            [FromQuery] string typeId,
            [FromQuery] string bookId)
        {
            var books = await _bookService.GetBooksByTypeIdAsync(typeId,Request);

            //Checked book exist in list
            var checkedBook = GetBookExisted(bookId, books);
            if (checkedBook != null)
                //Remove itself
                books.Remove(checkedBook);
            else
            {
                //Remove last element
                if (books.Count == 6)
                    books.RemoveRange(6, 1);
            }
            return Ok(books);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<EntityList<BooksViewModel>>> SearchBookByName(
            [FromQuery] string name,   
            [FromQuery] string typeId,
            [FromQuery] string sortPrice,
            [FromQuery] string publishHouseId,
            [FromQuery] string authorId,
            [FromQuery] string tag,
            [FromQuery] int page
        )
        {
            var filter = Builders<Book>.Filter.Regex("Alias", new BsonRegularExpression(Unsign.convertToUnSign(name).ToLower()))
                          & Builders<Book>.Filter.Eq("DeleteAt", BsonNull.Value);
            if (typeId != null)
                filter = filter & Builders<Book>.Filter.Eq("TypeId", typeId);
            if (publishHouseId != null)
                filter = filter & Builders<Book>.Filter.Eq("PublishHouseId", publishHouseId);
            if (authorId != null)
                filter = filter & Builders<Book>.Filter.Eq("AuthorId", authorId);
            if (tag != null)
                filter = filter & Builders<Book>.Filter.Eq("Tag", tag);
            SortDefinition<Book> sortDefinition = null;
            if (sortPrice == "desc")
                sortDefinition = Builders<Book>.Sort.Descending(x => x.Price);
            else
                sortDefinition = Builders<Book>.Sort.Ascending(x=>x.Price);
            var books = await _bookService.SearchBooksAsync(filter,sortDefinition,Request,page);
            return Ok(books);
        }
           
        

        [HttpGet("[action]")]
        public async Task<ActionResult<BookViewModel>> Get(
            [FromQuery(Name = "id")] string id
            )
        {
            var book = await _bookService.GetAsync(id);
            if (book == null)
            {
                return BadRequest("book not found");
            }
            var type = await _typeService.GetAsync(book.TypeId);
            var publishingHouse = await _publishingHouseService.GetAsync(book.PublishHouseId);
            var author = await _authorService.GetAsync(book.AuthorId);


            BookViewModel bookViewModel = new BookViewModel()
            {
                Id = book.Id,
                BookName = book.BookName,
                Description = book.Description,
                Price = book.Price.ToString(),
                CoverPrice = book.CoverPrice.ToString(),
                ImageSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, book.ImageName),
                Rating = Rouding.Adjust(Average.CountingAverage(book.Comments)),
                BookTypeName = type.Name,
                PublishingHouseName = publishingHouse.Name,
                Cover_Type = book.CoverType,
                PageAmount = book.PageAmount,
                PublishDate = Convert.ToDateTime(book.PublishDate).ToString("yyyy-MM-dd"),
                Size = book.Size,
                AuthorName = author.Name,
                AuthorId = author.Id,
                PublishingHouseId = publishingHouse.Id,
                TypeId = type.Id,
                Tag = book.Tag,
                Amount = book.Amount,
                ZoneType = book.ZoneType
            };
            
            return Ok(bookViewModel);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromForm] Book book)
        {
            book.ImageName = await SaveImageAsync(book.ImageFile);
            book.Alias = Unsign.convertToUnSign(book.BookName.ToLower());
            var createdBook = await _bookService.CreateAsync(book,Request);
            return Ok(createdBook);

        }
        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromForm] Book updatedBook)
        {
            var book = await _bookService.GetAsync(updatedBook.Id);
            updatedBook.ImageName = book.ImageName;

            if (updatedBook.ImageFile != null)
            {
                DeleteImage(book.ImageName);
                updatedBook.ImageName = await SaveImageAsync(updatedBook.ImageFile);
            }
           
            await _bookService.UpdateBookAsync(updatedBook);
            return Ok(updatedBook);
        }
        

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _bookService.GetAsync(id);
            book.DeleteAt = DateTime.UtcNow;
            await _bookService.UpdateAsync(id, book);
            return Ok("Delete sucessfully");
        }
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string imageName = new string(Path.GetFileNameWithoutExtension(imageFile.FileName).Take(10).ToArray()).Replace(' ', '-');
            imageName = imageName + DateTime.Now.ToString("yymmssff") + Path.GetExtension(imageFile.FileName);
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            using(var fileStream = new FileStream(imagePath,FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return imageName;
        }
        private  void DeleteImage(string imageName)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }
        private BooksViewModel GetBookExisted(string bookId, List<BooksViewModel> books)
        {
            foreach (var book in books)
                if (book.Id == bookId)
                    return book;
            return null;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<EntityList<BooksViewModel>>> SearchBookByNameAdmin(
            [FromQuery] string name,
            [FromQuery] int page
        )
        {
            var filter = Builders<Book>.Filter.Regex("Alias", new BsonRegularExpression(Unsign.convertToUnSign(name).ToLower()))
                          & Builders<Book>.Filter.Eq("DeleteAt", BsonNull.Value);
            var books = await _bookService.SearchBooksAdminAsync(filter, Request,page);
            return Ok(books);
        }

    }
}
