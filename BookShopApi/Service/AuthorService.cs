using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class AuthorService
    {
        private readonly IMongoCollection<Author> _authors;

        public AuthorService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _authors = database.GetCollection<Author>(settings.AuthorsCollectionName);
        }

        public async Task<List<Author>> GetAsync() =>
            await _authors.Find(author => true).ToListAsync();

        public async Task<Author> GetAsync(string id) =>
           await _authors.Find<Author>(author => author.Id == id).FirstOrDefaultAsync();
        public async Task<Author> CreateAsync(Author author)
        {
            await _authors.InsertOneAsync(author);
            return author;
        }

        public async Task UpdateAsync(string id, Author authorIn) =>
           await _authors.ReplaceOneAsync(author => author.Id == id, authorIn);


        public async Task RemoveAsync(string id) =>
           await _authors.DeleteOneAsync(author => author.Id == id);
    }
}
