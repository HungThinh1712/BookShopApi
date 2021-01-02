using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly ILogger<UserService> _logger;

        public UserService(IBookShopDatabaseSettings settings, ILogger<UserService> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _logger = logger;
            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public async Task<User> GetUserbyEmailAsync(string email) =>
            await _users.Find<User>(user => user.Email == email && user.DeleteAt==null).FirstOrDefaultAsync();
        

        public async Task<List<User>> GetAsync() =>
            await _users.Find(user => true).ToListAsync();

        public async Task<User> GetAsync(string id) =>
           await _users.Find<User>(user => user.Id == id).FirstOrDefaultAsync();
        public async Task<bool> GetAsyncByEmail(string email)
        {
            var user = await _users.Find<User>(user => user.Email == email).FirstOrDefaultAsync();
            return user != null;
        }
        public async Task<bool> GetAsyncByPhone(string phone)
        {
            var user = await _users.Find<User>(user => user.Phone == phone).FirstOrDefaultAsync();
            return user != null;
        }
        public async Task<User> CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task UpdateAsync(string id, User userIn) =>
           await _users.ReplaceOneAsync(user => user.Id == id, userIn);

        public async Task UpdateAddressAsync(string id, string name, string phone, string city,string district,string ward,string address)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.ProvinceId, city).
                                               Set(u=>u.FullName,name).
                                               Set(u=>u.Phone,phone).
                                               Set(u=>u.DistrictId,district).
                                               Set(u => u.WardId, ward).
                                               Set(u => u.SpecificAddress, address);
            await _users.UpdateOneAsync(filter,update);
        }
         
        public async Task RemoveAsync(string id) =>
           await _users.DeleteOneAsync(user => user.Id == id);

        public async Task UpdateCodeActiveAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.CodeActive, null);
            _logger.LogInformation(
                        "Queued Background Task {Guid} is starting.");
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task UpdateProfileAsync(string id, string name, string phone, int sex, DateTime birthday)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.Phone, phone).
                                               Set(u => u.FullName, name).
                                               Set(u => u.Sex, sex).
                                               Set(u => u.BirthDay, birthday);
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task UpdateProfileWithPassWordAsync(string id, string name, string phone, int sex, DateTime birthday, string newPassword)
        {
            string hassPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.Phone, phone).
                                               Set(u => u.FullName, name).
                                               Set(u => u.Sex, sex).
                                               Set(u => u.BirthDay, birthday).
                                               Set(u => u.PassWord, hassPassword);
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task<bool> CheckPassword(string id,string oldPassword)
        {
           
            var user = await _users.Find<User>(user => user.Id == id ).FirstOrDefaultAsync();

            //user not found or password incorrect or new password is the same with old password
            if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PassWord ))
                return false;
            return true;
        }
        
    }
}
