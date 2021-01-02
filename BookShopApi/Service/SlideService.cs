using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class SlideService
    {
        private readonly IMongoCollection<Slide> _slides;

        public SlideService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _slides = database.GetCollection<Slide>(settings.SlidesCollectionName);
        }

        public async Task<List<Slide>> GetAsync() =>
            await _slides.Find(slide => true).ToListAsync();

        public async Task<Slide> GetAsync(string id) =>
           await _slides.Find<Slide>(slide => slide.Id == id).FirstOrDefaultAsync();

        public async Task<Slide> CreateAsync(Slide slide)
        {
            await _slides.InsertOneAsync(slide);
            return slide;
        }

        public async Task UpdateAsync(string id, Slide slideIn) =>
           await _slides.ReplaceOneAsync(slide => slide.Id == id, slideIn);


        public async Task RemoveAsync(string id) =>
           await _slides.DeleteOneAsync(book => book.Id == id);
    }
}
