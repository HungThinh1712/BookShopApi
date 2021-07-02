using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels;
using BookShopApi.Models.ViewModels.Books;
using BookShopApi.Models.ViewModels.BookTypes;
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
        private readonly TagService _tagService;
        private readonly IWebHostEnvironment _hostEnvironment;
        public BooksController(BookService bookService,
                               TypeService typeService,
                               PublishingHouseService publishingHouseService,
                               AuthorService authorService,
                               TagService tagService,
                               IWebHostEnvironment hostEnvironment)
        {
            _bookService = bookService;
            _typeService = typeService;
            _publishingHouseService = publishingHouseService;
            _authorService = authorService;
            _tagService = tagService;
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
            [FromQuery] int index,
            [FromQuery] string zoneType,
            [FromQuery] string tag)
        {
            var books = await _bookService.GetByZoneAsync(index, Request, zoneType, tag);

            return Ok(books);
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<List<BooksViewModel>>> GetBookByType(
            [FromQuery] int index,
            [FromQuery] string type)
        {
            if (!string.IsNullOrEmpty(type))
            {
                var books = await _bookService.GetByTypeAsync(index, Request, type);

                return Ok(books);
            }
            return Ok(new List<BooksViewModel>());
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
            var books = await _bookService.GetBooksByTypeIdAsync(typeId, Request);

            //Checked book exist in list
            if (books.Count > 0)
            {
                var checkedBook = GetBookExisted(bookId, books);
                if (checkedBook != null)
                    //Remove itself
                    books.Remove(checkedBook);
                else
                {
                    //Remove last element
                    if (books.Count == 6)
                        books.RemoveRange(5, 1);
                }
            }
            return Ok(books);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<BooksViewModel>>> GetAllBook()
        {
            var books = await _bookService.GetAllAsync();

            //Checked book exist in list
          
            return Ok(books);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<EntityList<BooksViewModel>>> SearchBookByName(
            [FromQuery] string name,
            [FromQuery] string typeId,
            [FromQuery] string sortPrice,
            [FromQuery] string publishHouseId,
            [FromQuery] string authorId,
            [FromQuery] string tagId,
            [FromQuery] int page
        )
        {
            var filter = Builders<Book>.Filter.Text(name);
            if (typeId != null)
                filter = filter & Builders<Book>.Filter.Eq("TypeId", typeId);
            if (publishHouseId != null)
                filter = filter & Builders<Book>.Filter.Eq("PublishHouseId", publishHouseId);
            if (authorId != null)
                filter = filter & Builders<Book>.Filter.Eq("AuthorId", authorId);
            if (tagId != null)
                filter = filter & Builders<Book>.Filter.Eq("TagId", tagId);
            SortDefinition<Book> sortDefinition = null;
            if (sortPrice == "desc")
                sortDefinition = Builders<Book>.Sort.Descending(x => x.Price);
            else
                sortDefinition = Builders<Book>.Sort.Ascending(x => x.Price);
            filter = filter & Builders<Book>.Filter.Eq("DeleteAt", BsonNull.Value);
            var books = await _bookService.SearchBooksAsync(filter, sortDefinition, Request, page);
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
            var author = await _authorService.GetAsync(book.AuthorId, Request);
            var tag = await _tagService.GetAsync(book.TagId);


            BookViewModel bookViewModel = new BookViewModel()
            {
                Id = book.Id,
                BookName = book.BookName,
                Description = book.Description,
                Price = book.Price.ToString(),
                CoverPrice = book.CoverPrice.ToString(),
                ImgUrl = book.ImgUrl,
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
                TagId = book.TagId,
                TagName = tag.Name,
                Amount = book.Amount,
                ZoneType = book.ZoneType
            };

            return Ok(bookViewModel);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromForm] Book book)
        {
            book.Alias = Unsign.convertToUnSign(book.BookName.ToLower());
            var createdBook = await _bookService.CreateAsync(book, Request);
            return Ok(createdBook);

        }
        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromForm] Book updatedBook)
        {

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
        [HttpPut("[action]")]
        public async Task UpdateMany()
        {
            await _bookService.UpdateManyAsyns();
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
            var books = await _bookService.SearchBooksAdminAsync(filter, Request, page);
            return Ok(books);
        }
        [HttpGet("[action]")]
        public async Task<ActionResult> CorrectDataBook()
        {
            var books = await _bookService.GetAllBooksAsync();
            await _bookService.DeleteManyAsync();
            await _bookService.AddManyAsync(books);
            return Ok();
        }
    }
}
