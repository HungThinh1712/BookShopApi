using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class DistrictService
    {
        private readonly IMongoCollection<District> _districts;

        public DistrictService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _districts = database.GetCollection<District>(settings.DistrictsCollectionName);
        }


        public async Task<District> CreateAsync(District district)
        {
            await _districts.InsertOneAsync(district);
            return district;
        }
        public async Task<bool> CreateManyAsync(List<District> districts)
        {
            await _districts.InsertManyAsync(districts);
            return true;
        }
        public async Task<List<District>> GetByProvinceIdAsync(string id)
        {
            return await _districts.Find(x => x.ProvinceId == id).ToListAsync();
        }
        public async Task<District> GetByIdAsync(string id)
        {
            return await _districts.Find(x => x.Id == id).FirstOrDefaultAsync() ;
        }
    }
}
