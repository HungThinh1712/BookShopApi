using BookShopApi.DatabaseSettings;
using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels;
using Mapster;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace BookShopApi.Service
{
    public class BookService
    {
        private readonly IMongoCollection<Book> _books;
        public BookService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _books = database.GetCollection<Book>(settings.BooksCollectionName);
            _books.Indexes.CreateOne(new CreateIndexModel<Book>(Builders<Book>.IndexKeys.Text(x => x.BookName)));

        }

        public async Task<List<BooksViewModel>> GetAsync(int index, HttpRequest request)
        {
            //Get ten books first time
            int size = 10;
            int length = 10;
            //Get extra five books after first time
            return await _books.Find(book => book.DeleteAt == null && book.Amount > 0).Limit(index * size + length).Project(x =>
                                     new BooksViewModel
                                     {
                                         Id = x.Id,
                                         BookName = x.BookName,
                                         Price = x.Price.ToString(),
                                         CoverPrice = x.CoverPrice.ToString(),
                                         ImgUrl = x.ImgUrl,
                                         Rating = Rouding.Adjust(Average.CountingAverage(x.Comments)),
                                         TypeId = x.TypeId
                                     }).ToListAsync();
        }
        public async Task<List<Book>> GetAllAsync()
        {

            return await _books.Find(book => book.DeleteAt == null && book.Amount > 0).ToListAsync();
        }

        public async Task<List<BooksViewModel>> GetByZoneAsync(int index, HttpRequest request, string zoneType, string tag)
        {
            int size = 5;
            int length = 5;
            return await _books.Find(book => book.DeleteAt == null && book.ZoneType == zoneType && book.TagId == tag && book.Amount > 0
                    ).Limit(index * size + length).Project(x =>
                                   new BooksViewModel
                                   {
                                       Id = x.Id,
                                       BookName = x.BookName,
                                       Price = x.Price.ToString(),
                                       CoverPrice = x.CoverPrice.ToString(),
                                       ImgUrl = x.ImgUrl,
                                       Rating = Rouding.Adjust(Average.CountingAverage(x.Comments)),
                                       CountRating = x.Comments.Count,
                                       TypeId = x.TypeId
                                   }).ToListAsync();
        }

        public async Task<List<BooksViewModel>> GetByTagAsync(int index, HttpRequest request, string tag)
        {
            //Get ten books first time
            int size = 10;
            int length = 10;
            //Get extra five books after first time
            return await _books.Find(book => book.DeleteAt == null && book.TagId == tag).Limit(index * size + length).Project(x =>
                                    new BooksViewModel
                                    {
                                        Id = x.Id,
                                        BookName = x.BookName,
                                        Price = x.Price.ToString(),
                                        CoverPrice = x.CoverPrice.ToString(),
                                        ImgUrl = x.ImgUrl,
                                        Rating = Rouding.Adjust(Average.CountingAverage(x.Comments)),
                                        TypeId = x.TypeId
                                    }).ToListAsync();
        }

        public async Task<List<BooksViewModel>> GetBooksByTypeIdAsync(string typeId, HttpRequest request)
        {
            int size = 6;  //Get ten books first time
            return await _books.Find(book => book.TypeId == typeId).Limit(size).Project(x =>
                                  new BooksViewModel
                                  {
                                      Id = x.Id,
                                      BookName = x.BookName,
                                      Price = x.Price.ToString(),
                                      CoverPrice = x.CoverPrice.ToString(),
                                      ImgUrl = x.ImgUrl,
                                      Rating = Rouding.Adjust(Average.CountingAverage(x.Comments)),
                                      CountRating = x.Comments.Count,
                                      TypeId = x.TypeId
                                  }).ToListAsync();
        }

        public async Task<EntityList<BooksViewModel>> SearchBooksAsync(FilterDefinition<Book> filter = null, SortDefinition<Book> sort = null, HttpRequest request = null, int page = 1)
        {
            int pageSize = 10;
            var query = _books.Find(filter);
            var total = await query.CountDocumentsAsync();
            query = query.Skip((page - 1) * pageSize).Limit(pageSize).Sort(sort);

            return new EntityList<BooksViewModel>
            {
                Total = (int)total,
                Entities = await query.Project(x =>
                                    new BooksViewModel
                                    {
                                        Id = x.Id,
                                        BookName = x.BookName,
                                        Price = x.Price.ToString(),
                                        CoverPrice = x.CoverPrice.ToString(),
                                        ImgUrl = x.ImgUrl,
                                        Rating = Rouding.Adjust(Average.CountingAverage(x.Comments)),
                                        CountRating = x.Comments.Count,
                                        TypeId = x.TypeId
                                    }).ToListAsync()
            };
        }

        public async Task<int> CountBooks(FilterDefinition<Book> filter = null)
        {
           
            var query = _books.Find(filter);
            var total = await query.CountDocumentsAsync();
            return (int)total;
           
        }

        public async Task<Book> GetAsync(string id) =>
           await _books.Find<Book>(book => book.Id == id).FirstOrDefaultAsync();
        public async Task<List<Book>> GetAllBooksAsync() =>
          await _books.Find<Book>(book => true).ToListAsync();
        public async Task DeleteManyAsync()
        {
            await _books.DeleteManyAsync(x => true);
        }
        public async Task AddManyAsync(List<Book> books)
        {
            await _books.InsertManyAsync(books);
        }


        public async Task<BooksViewModel> CreateAsync(Book book, HttpRequest request)
        {
            book.Code = DateTime.Now.ToString("yymmssff") + book.PublishDate.DayOfYear.ToString().PadLeft(3, '0');
            await _books.InsertOneAsync(book);
            return new BooksViewModel()
            {
                Id = book.Id,
                BookName = book.BookName,
                Price = book.Price.ToString(),
                CoverPrice = book.CoverPrice.ToString(),
                ImgUrl = book.ImgUrl,
                Rating = Rouding.Adjust(Average.CountingAverage(book.Comments)),
                TypeId = book.TypeId
            };

        }

        public async Task UpdateBookAsync(Book updatedBook)
        {
            var filter = Builders<Book>.Filter.Eq(x => x.Id, updatedBook.Id);
            var update = Builders<Book>.Update.Set(x => x.BookName, updatedBook.BookName).
                                                Set(x => x.TagId, updatedBook.TagId).
                                                Set(x => x.PublishHouseId, updatedBook.PublishHouseId).
                                                Set(x => x.AuthorId, updatedBook.AuthorId).
                                                Set(x => x.TypeId, updatedBook.TypeId).
                                                Set(x => x.PublishDate, updatedBook.PublishDate).
                                                Set(x => x.Amount, updatedBook.Amount).
                                                Set(x => x.Price, updatedBook.Price).
                                                Set(x => x.CoverPrice, updatedBook.CoverPrice).
                                                Set(x => x.PageAmount, updatedBook.PageAmount).
                                                Set(x => x.Size, updatedBook.Size).
                                                Set(x => x.CoverType, updatedBook.CoverType).
                                                Set(x => x.ZoneType, updatedBook.ZoneType).
                                                Set(x => x.ImgUrl, updatedBook.ImgUrl);
            await _books.UpdateOneAsync(filter, update);
        }

        public async Task UpdateAsync(string id, Book bookIn) =>
           await _books.ReplaceOneAsync(book => book.Id == id, bookIn);


        public async Task RemoveAsync(string id) =>
           await _books.DeleteOneAsync(book => book.Id == id);


        public async Task<EntityList<BooksViewModel>> SearchBooksAdminAsync(FilterDefinition<Book> filter = null, HttpRequest request = null, int page = 1)
        {
            int pageSize = 16;
            var query = _books.Find(filter);
            var total = await query.CountDocumentsAsync();
            query = query.Skip((page - 1) * pageSize).Limit(pageSize);

            return new EntityList<BooksViewModel>
            {
                Total = (int)total,
                Entities = await query.Project(x =>
                                    new BooksViewModel
                                    {
                                        Id = x.Id,
                                        BookName = x.BookName,
                                        Price = x.Price.ToString(),
                                        CoverPrice = x.CoverPrice.ToString(),
                                        ImgUrl = x.ImgUrl,
                                        Rating = Rouding.Adjust(Average.CountingAverage(x.Comments)),
                                        TypeId = x.TypeId
                                    }).ToListAsync()
            };
        }

        public async Task UpdateManyAsyns()
        {
            await _books.UpdateManyAsync(x => true, MongoDB.Driver.Builders<Book>.Update.Set(x => x.TagId, "Sách bán chạy trong ngày"));
        }

        public async Task<List<BooksViewModel>> GetByTypeAsync(int index, HttpRequest request, string type)
        {
            //Get ten books first time
            int size = 5;
            int length = 5;
            //Get extra five books after first time
            return await _books.Find(book => book.DeleteAt == null && book.TypeId == type && book.Amount > 0).Limit(index * size + length).Project(x =>
                                     new BooksViewModel
                                     {
                                         Id = x.Id,
                                         BookName = x.BookName,
                                         Price = x.Price.ToString(),
                                         CoverPrice = x.CoverPrice.ToString(),
                                         ImgUrl = x.ImgUrl,
                                         Rating = Rouding.Adjust(Average.CountingAverage(x.Comments)),
                                         CountRating = x.Comments.Count,
                                         TypeId = x.TypeId
                                     }).ToListAsync();
        }
    }
}
