﻿using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.BookTypes;
using BookShopApi.Models.ViewModels.Types;
using Mapster;
using MongoDB.Driver;
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

        public async Task<List<BookTypeViewModel>> GetAsync()
        {
            return (await _types.Find(type => type.DeleteAt == null).ToListAsync()).Adapt<List<BookTypeViewModel>>();
        }
          
        public async Task<BookType> GetAsync(string id) =>
           await _types.Find<BookType>(type => type.Id == id).FirstOrDefaultAsync();

        public async Task<BookType> CreateAsync(BookType type)
        {
            await _types.InsertOneAsync(type);
            return type;
        }

        public async Task UpdateAsync(string id, BookType typeIn) =>
           await _types.ReplaceOneAsync(type => type.Id == id, typeIn);

        public async Task RemoveAsync(string id) =>
            await _types.DeleteOneAsync(type => type.Id == id);

        public async Task<EntityList<TypesInAdminViewModel>> GetAllTypeAsync(string name, int page = 1, int pageSize = 10)
        {
            string searchName = string.IsNullOrEmpty(name) ? string.Empty : name.ToLower();
            var query = _types.Find(author => author.Name.ToLower().Contains(searchName));

            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<TypesInAdminViewModel>()
            {
                Total = (int)total,
                Entities = (await query.ToListAsync()).Adapt<List<TypesInAdminViewModel>>()
            };
        }
    }
}