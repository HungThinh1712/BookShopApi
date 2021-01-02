using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Services
{
    public class TagService
    {
        private readonly IMongoCollection<Models.Tag> _tags;

        public TagService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _tags = database.GetCollection<Models.Tag>(settings.TagsCollectionName);
        }

        public async Task<List<Models.Tag>> GetAsync() =>
           await _tags.Find(tag => tag.IsDeleted == false).ToListAsync();
        public async Task<Models.Tag> GetAsync(string id) =>
           await _tags.Find(tag => tag.Id == id && tag.IsDeleted == false).FirstOrDefaultAsync();

        public async Task<Models.Tag> CreateAsync(Models.Tag tag)
        {
            await _tags.InsertOneAsync(tag);
            return tag;
        }

        public async Task UpdateAsync(string id, Models.Tag tagIn) =>
           await _tags.ReplaceOneAsync(tag => tag.Id == id, tagIn);

        public async Task RemoveAsync(string id) =>
            await _tags.DeleteOneAsync(tag => tag.Id == id);
    }
}