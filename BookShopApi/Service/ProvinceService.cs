using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class ProvinceService
    {
        private readonly IMongoCollection<Province> _provinces;

        public ProvinceService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _provinces = database.GetCollection<Province>(settings.ProvincesCollectionName);
        }

        
        public async Task<Province> CreateAsync(Province province)
        {
            await _provinces.InsertOneAsync(province);
            return province;
        }
        public async Task<List<Province>> GetAllAsync()
        {
           return await _provinces.Find(x=>true).ToListAsync();
        }
        public async Task<Province> GetByIdAsync(string id)
        {
            return await _provinces.Find(x => x.Id ==id).FirstOrDefaultAsync();
        }
    }
}
