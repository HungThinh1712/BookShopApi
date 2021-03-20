using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using BookShopApi.Models.ViewModels.Authors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;

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

        public async Task<List<Models.Author>> GetAsync() =>
            await _authors.Find(author => true).ToListAsync();

        public async Task<Models.Author> GetAsync(string id) =>
           await _authors.Find<Models.Author>(author => author.Id == id).FirstOrDefaultAsync();
        public async Task<Models.Author> CreateAsync(Models.Author author)
        {
            await _authors.InsertOneAsync(author);
            return author;
        }

        public async Task UpdateAsync(string id, string name)
        {
            var filter = Builders<Author>.Filter.Eq(u => u.Id, id);
            var update = Builders<Author>.Update.Set(u => u.Name, name);
            await _authors.UpdateOneAsync(filter, update);
        }

        public async Task RemoveAsync(string id) =>
           await _authors.DeleteOneAsync(author => author.Id == id);

        public async Task<EntityList<AuthorsInAdminViewModel>> GetAllAuthorAsync(string name, int page = 1, int pageSize = 10)
        {
            string searchName = string.IsNullOrEmpty(name) ? string.Empty : name.ToLower();
            var query = _authors.Find(author => author.Name.ToLower().Contains(searchName));

            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<AuthorsInAdminViewModel>()
            {
                Total = (int)total,
                Entities = (await query.ToListAsync()).Adapt<List<AuthorsInAdminViewModel>>()
            };
        }
    }
}