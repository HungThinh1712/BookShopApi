using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.PublishingHouses;
using Mapster;
using Microsoft.AspNetCore.Http;
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

        public async Task<EntityList<PublishingHousesInAdminViewModel>> GetAsync(string name, int page, int pageSize, HttpRequest request)
        {
            string searchString = String.IsNullOrEmpty(name) ? string.Empty : name;
            var query = _publishingHouses.Find(publishingHouse => publishingHouse.Name.Contains(searchString) && publishingHouse.DeleteAt==null)
                .Project(x => new PublishingHousesInAdminViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreateAt = x.CreateAt.ToLocalTime().ToString("yyyy-MM-dd"),
                }); ;
            int total = (int)await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<PublishingHousesInAdminViewModel>()
            {
                Total = total,
                Entities = await query.ToListAsync()
            };;
        }
           
        public async Task<Models.PublishingHouse> GetAsync(string id) =>
           await _publishingHouses.Find<Models.PublishingHouse>(publishingHouse => publishingHouse.Id == id).FirstOrDefaultAsync();

        public async Task<Models.PublishingHouse> CreateAsync(Models.PublishingHouse publishingHouse)
        {
            await _publishingHouses.InsertOneAsync(publishingHouse);
            return publishingHouse;
        }

        public async Task<PublishingHouse> UpdateAsync(PublishingHouse publishingHouse)
        {
            var filter = Builders<PublishingHouse>.Filter.Eq(u => u.Id, publishingHouse.Id);
            var update = Builders<PublishingHouse>.Update.Set(u => u.Name, publishingHouse.Name);
            await _publishingHouses.UpdateOneAsync(filter, update);
            var result = await _publishingHouses.Find<Models.PublishingHouse>(x => x.Id == publishingHouse.Id).FirstOrDefaultAsync();
            return result;
        }

        public async Task RemoveAsync(string id)
        {
            var filter = Builders<PublishingHouse>.Filter.Eq(u => u.Id,id);
            var update = Builders<PublishingHouse>.Update.Set(u => u.DeleteAt, DateTime.UtcNow);
            await _publishingHouses.UpdateOneAsync(filter, update);
        }
           

        public async Task<EntityList<PublishingHousesInAdminViewModel>> GetAllTypeAsync(string name, int page = 1, int pageSize = 10)
        {
            string searchName = string.IsNullOrEmpty(name) ? string.Empty : name.ToLower();
            var query = _publishingHouses.Find(author => author.Name.ToLower().Contains(searchName) && author.DeleteAt==null);

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