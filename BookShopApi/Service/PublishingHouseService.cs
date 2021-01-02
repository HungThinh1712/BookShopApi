using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class PublishingHouseService
    {
        private readonly IMongoCollection<PublishingHouse> _publishingHouses;

        public PublishingHouseService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _publishingHouses = database.GetCollection<PublishingHouse>(settings.PublishingHousesCollectionName);
        }

        public async Task<List<PublishingHouse>> GetAsync() =>
            await _publishingHouses.Find(publishingHouse => true).ToListAsync();

        public async Task<PublishingHouse> GetAsync(string id) =>
           await _publishingHouses.Find<PublishingHouse>(publishingHouse => publishingHouse.Id == id).FirstOrDefaultAsync();

        public async Task<PublishingHouse> CreateAsync(PublishingHouse publishingHouse)
        {
            await _publishingHouses.InsertOneAsync(publishingHouse);
            return publishingHouse;
        }

        public async Task UpdateAsync(string id, PublishingHouse publishingHouseIn) =>
           await _publishingHouses.ReplaceOneAsync(publishingHouse => publishingHouse.Id == id, publishingHouseIn);


        public async Task RemoveAsync(string id) =>
           await _publishingHouses.DeleteOneAsync(publishingHouse => publishingHouse.Id == id);
    }
}
