using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.BookTypes;
using BookShopApi.Models.ViewModels.Types;
using Mapster;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class TypeService
    {
        private readonly IMongoCollection<BookType> _types;

        public TypeService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _types = database.GetCollection<BookType>(settings.TypesCollectionName);
        }

        public async Task<EntityList<BookTypeViewModel>> GetAsync(string name, int page, int pageSize)
        {
            string searchString = string.IsNullOrEmpty(name) ? string.Empty : name;
            var query = _types.Find(type =>type.Name.Contains(searchString) && type.DeleteAt==null).Project(x => new BookTypeViewModel
            {
                Id = x.Id,
                Name = x.Name,
                CreateAt = x.CreateAt.ToLocalTime().ToString("yyyy-MM-dd"),
            }); 
            int total = (int)await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<BookTypeViewModel>()
            {
                Total = total ,
                Entities = (await query.ToListAsync()).Adapt<List<BookTypeViewModel>>()
            };
           
        }
          
        public async Task<BookType> GetAsync(string id) =>
           await _types.Find<BookType>(type => type.Id == id).FirstOrDefaultAsync();

        public async Task<bool> CreateAsync(BookType type)
        {
            await _types.InsertOneAsync(type);
            return true;
        }

        public async Task<BookType> UpdateAsync(string id, BookType typeIn)
        {
            var update = Builders<BookType>.Update.Set(x => x.Name, typeIn.Name);
            await _types.UpdateOneAsync(type => type.Id == id, update);
            var result = await _types.Find<BookType>(type => type.Id == id).FirstOrDefaultAsync();
            return result;
        }

        public async Task RemoveAsync(string id)
        {
            var update = Builders<BookType>.Update.Set(x => x.DeleteAt, DateTime.UtcNow);
            await _types.UpdateOneAsync(type => type.Id == id, update);
        }

        public async Task<EntityList<TypesInAdminViewModel>> GetAllTypeAsync(string name, int page = 1, int pageSize = 10)
        {
            string searchName = string.IsNullOrEmpty(name) ? string.Empty : name.ToLower();
            var query = _types.Find(author => author.Name.ToLower().Contains(searchName) && author.DeleteAt==null);

            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<TypesInAdminViewModel>()
            {
                Total = (int)total,
                Entities = (await query.ToListAsync()).Adapt<List<TypesInAdminViewModel>>()
            };
        }

        public async Task UpdateAsync(string id, string name)
        {
            var filter = Builders<BookType>.Filter.Eq(u => u.Id, id);
            var update = Builders<BookType>.Update.Set(u => u.Name, name);
            await _types.UpdateOneAsync(filter, update);
        }
    }
}