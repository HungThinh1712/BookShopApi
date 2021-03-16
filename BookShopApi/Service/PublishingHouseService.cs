using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.PublishingHouses;
using Mapster;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class PublishingHouseService
    {
        private readonly IMongoCollection<Models.PublishingHouse> _publishingHouses;

        public PublishingHouseService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _publishingHouses = database.GetCollection<Models.PublishingHouse>(settings.PublishingHousesCollectionName);
        }

        public async Task<List<Models.PublishingHouse>> GetAsync() =>
            await _publishingHouses.Find(publishingHouse => true).ToListAsync();

        public async Task<Models.PublishingHouse> GetAsync(string id) =>
           await _publishingHouses.Find<Models.PublishingHouse>(publishingHouse => publishingHouse.Id == id).FirstOrDefaultAsync();

        public async Task<Models.PublishingHouse> CreateAsync(Models.PublishingHouse publishingHouse)
        {
            await _publishingHouses.InsertOneAsync(publishingHouse);
            return publishingHouse;
        }

        public async Task UpdateAsync(string id, string name)
        {
            var filter = Builders<PublishingHouse>.Filter.Eq(p => p.Id, id);
            var update = Builders<PublishingHouse>.Update.Set(p => p.Name, name);
            await _publishingHouses.UpdateOneAsync(filter, update);
        }

        public async Task RemoveAsync(string id) =>
           await _publishingHouses.DeleteOneAsync(publishingHouse => publishingHouse.Id == id);

        public async Task<EntityList<PublishingHousesInAdminViewModel>> GetAllTypeAsync(string name, int page = 1, int pageSize = 10)
        {
            string searchName = string.IsNullOrEmpty(name) ? string.Empty : name.ToLower();
            var query = _publishingHouses.Find(author => author.Name.ToLower().Contains(searchName));

            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<PublishingHousesInAdminViewModel>()
            {
                Total = (int)total,
                Entities = (await query.ToListAsync()).Adapt<List<PublishingHousesInAdminViewModel>>()
            };
        }
    }
}