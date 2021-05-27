using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using BookShopApi.Models.ViewModels.Authors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Http;

namespace BookShopApi.Service
{
    public class AuthorService
    {
        private readonly IMongoCollection<Models.Author> _authors;

        public AuthorService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _authors = database.GetCollection<Models.Author>(settings.AuthorsCollectionName);
        }

        public async Task<EntityList<AuthorsInAdminViewModel>> GetAsync(string name, int page,int pageSize, HttpRequest request)
        {
            string searchString = String.IsNullOrEmpty(name) ? string.Empty : name;
            var query = _authors.Find(author => author.Name.Contains(searchString) && author.DeleteAt==null)
                .Project(x=>new AuthorsInAdminViewModel 
                { 
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    BirthDay = x.BirthDay.ToLocalTime().ToString("yyyy-MM-dd"),
                    ImageSrc = String.Format("{0}://{1}{2}/Images/{3}", request.Scheme, request.Host, request.PathBase, x.ImageName),
                });
            int total = (int)await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<AuthorsInAdminViewModel>()
            {
                Total = total,
                Entities = await query.ToListAsync()
            };
        }

        public async Task<AuthorsInAdminViewModel> GetAsync(string id,HttpRequest request) =>
           await _authors.Find<Models.Author>(author => author.Id == id).Project(x => new AuthorsInAdminViewModel
           {
               Id = x.Id,
               Name = x.Name,
               Description = x.Description,
               BirthDay = x.BirthDay.ToLocalTime().ToString("yyyy-MM-dd"),
               ImageSrc = String.Format("{0}://{1}{2}/Images/{3}", request.Scheme, request.Host, request.PathBase, x.ImageName),
           }).FirstOrDefaultAsync();
        public async Task<Author> GetAsync(string id) =>
          await _authors.Find<Models.Author>(author => author.Id == id).FirstOrDefaultAsync();
        public async Task<bool> CreateAsync(Models.Author author, HttpRequest request)
        {
            await _authors.InsertOneAsync(author);
            var query = _authors.Find(author => true).SortByDescending(x => x.CreateAt);
            return true;
        }

        public async Task<Author> UpdateAsync(Author author)
        {
            
            await _authors.ReplaceOneAsync(x=>x.Id ==author.Id, author);
            return author;
           
        }

        public async Task RemoveAsync(string id)
        {
            var update = Builders<Author>.Update.Set(x => x.DeleteAt, DateTime.UtcNow);
            await _authors.UpdateOneAsync(x => x.Id == id, update);
        }
    }
}