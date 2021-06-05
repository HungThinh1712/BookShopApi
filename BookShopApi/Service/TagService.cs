using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Service
{

    public class TagService
    {
        private readonly IMongoCollection<BookTag> _tags;

        public TagService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _tags = database.GetCollection<BookTag>(settings.TagsCollectionName);
        }

        public async Task<List<BookTag>> GetAsync()
        {
            return await _tags.Find<BookTag>(x => true).ToListAsync();
        }

        public async Task<bool> CreateAsync(BookTag tag)
        {
            await _tags.InsertOneAsync(tag);
            return true;
        }
        public async Task<BookTag> GetAsync(string id)
        {
            return await _tags.Find<BookTag>(x => x.Id == id).FirstOrDefaultAsync();

        }

    }
}
