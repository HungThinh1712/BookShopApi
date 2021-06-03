using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Users;
using Mapster;
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
            await _users.Find<User>(user => user.Email == email).FirstOrDefaultAsync();
        

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
        public async Task<User> CreateAsync(string email,string fullName )
        {
            var user = new User();

            user.Email = email;
            user.FullName = fullName;
            user.ImgUrl = "https://www.pphfoundation.ca/wp-content/uploads/2018/05/default-avatar.png";
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

        public async Task<User> GetAdminAsync() =>
         await _users.Find<User>(user => user.IsAdmin == true).FirstOrDefaultAsync();

        public async Task  UpdateAvatarAsync(UpdateAvatarModel updatedUser)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, updatedUser.Id);
            var update = Builders<User>.Update.Set(u => u.ImgUrl, updatedUser.ImgUrl);
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task UpdateCodePasswordAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.CodeResetPassWord, null);
            _logger.LogInformation(
                        "Queued Background Task {Guid} is starting.");
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task<EntityList<UsersInAdminViewModel>> GetAllAsync(string name ,int page = 1, int pageSize = 10)
        {
            string searchName = string.IsNullOrEmpty(name) ? string.Empty : name.ToLower();
            var query = _users.Find(user => user.FullName.ToLower().Contains(searchName) && user.IsAdmin==false);

            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<UsersInAdminViewModel>()
            {
                Total = (int)total,
                Entities = (await query.ToListAsync()).Adapt<List<UsersInAdminViewModel>>()
            };
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _users.Find<User>(user => user.Email == email).FirstOrDefaultAsync();
            return user ;
        }

        internal Task GetAllUserAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
