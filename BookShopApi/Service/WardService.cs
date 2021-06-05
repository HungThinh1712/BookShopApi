using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class WardService
    {
        private readonly IMongoCollection<Ward> _wards;

        public WardService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _wards = database.GetCollection<Ward>(settings.WardsCollectionName);
        }


        public async Task<Ward> CreateAsync(Ward ward)
        {
            await _wards.InsertOneAsync(ward);
            return ward;
        }
        public async Task<bool> CreateManyAsync(List<Ward> wards)
        {
            await _wards.InsertManyAsync(wards);
            return true;
        }
        public async Task<List<Ward>> GetByDistrictIdAsync(string id)
        {
            return await _wards.Find(x => x.DistrictId == id).ToListAsync();
        }
        public async Task<Ward> GetByIdAsync(string id)
        {
            return await _wards.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
    }
}
