using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Users;
using Mapster;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Text;
using System.Net.Mail;

namespace BookShopApi.Service
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly ILogger<UserService> _logger;
        private readonly IHostEnvironment _webHostEnvironment;

        public UserService(IBookShopDatabaseSettings settings, ILogger<UserService> logger, IHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _logger = logger;
            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public async Task<User> GetUserbyEmailAsync(string email) =>
            await _users.Find<User>(user => user.Email == email && user.DeleteAt ==null && user.IsActive ==true).FirstOrDefaultAsync();

        public async Task<User> GetUserLoginbyEmailAsync(string email) =>
           await _users.Find<User>(user => user.Email == email && user.DeleteAt == null).FirstOrDefaultAsync();


        public async Task<List<User>> GetAsync() =>
            await _users.Find(user => true && user.IsAdmin==false && user.IsActive==true).ToListAsync();
        public async Task<List<AllUserVM>> GetAllAsync() =>
           (await _users.Find(user => true && user.IsAdmin == false && user.IsActive == true).ToListAsync()).Adapt<List<AllUserVM>>();

        public async Task<User> GetAsync(string id) =>
           await _users.Find<User>(user => user.Id == id).FirstOrDefaultAsync();
        public async Task<bool> GetAsyncByEmail(string email)
        {
            var user = await _users.Find<User>(user => user.Email == email).FirstOrDefaultAsync();
            return user != null;
        }
        public async Task<bool> GetAdminAsyncByEmail(string email)
        {
            var user = await _users.Find<User>(user => user.Email == email && user.IsAdmin==true).FirstOrDefaultAsync();
            return user != null;
        }

        public async Task<bool> DeactivateOrActivateAdmin(string id,bool isActivate)
        {
            var update = Builders<User>.Update.Set(x => x.IsActive, isActivate);
            await _users.UpdateOneAsync(x => x.Id == id, update);
            return true;
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
            user.IsActive = true;
            user.FullName = fullName;
            user.ImgUrl = "https://www.pphfoundation.ca/wp-content/uploads/2018/05/default-avatar.png";
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<User> CreateAdminAsync(string email, string fullName,string phoneNumber)
        {
            var user = new User();

            user.Email = email;
            user.IsActive = true;
            user.FullName = fullName;
            user.ImgUrl = "https://www.pphfoundation.ca/wp-content/uploads/2018/05/default-avatar.png";
            user.IsAdmin = true;
            user.Phone = phoneNumber;
            await _users.InsertOneAsync(user);
            return user;
        }


        public async Task UpdateAsync(string id, User userIn) =>
           await _users.ReplaceOneAsync(user => user.Id == id, userIn);

        public async Task UpdateAddressAsync(string id, string name, string phone,string address,string provinceId,string districtId,string wardId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.
                                               Set(u => u.FullName, name).
                                               Set(u => u.Phone, phone).
                                               Set(u => u.SpecificAddress, address)
                                               .Set(x=>x.ProvinceId,provinceId)
                                               .Set(x=>x.DistrictId,districtId).
                                               Set(x=>x.WardId,wardId);
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

        public async Task<List<User>> GetAdminAsync() =>
         await _users.Find<User>(user => user.IsAdmin == true).ToListAsync();

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
        public async Task UpdateAdminAsync(UpdatedcAdmin admin)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, admin.Id);
            var update = Builders<User>.Update.Set(u => u.IsTopAdmin, admin.IsTopAdmin)
                .Set(u=>u.Phone,admin.PhoneNumber)
                .Set(u=>u.Email,admin.Email).
                Set(u=>u.FullName,admin.FullName);
            
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

        public async Task<List<User>> GetAdminsAsync(string name)
        {
            var searchString = name ?? string.Empty;
            return await _users.Find<User>(x => x.IsAdmin == true && x.DeleteAt == null && x.FullName.Contains(searchString)).ToListAsync();
        }
        public async Task<bool> CreateAdminsAsync(AdminRM admin)
        {
            var password = RandomPassword();
            var user = new User
            {
                FullName = admin.FullName,
                Phone = admin.PhoneNumber,
                Email = admin.Email,
                IsActive = true,
                IsTopAdmin = admin.IsTopAdmin,
                IsAdmin = true,
                PassWord = BCrypt.Net.BCrypt.HashPassword(password)
            };
            await _users.InsertOneAsync(user);
           
                await SendMailAsync(admin.Email, password, admin.FullName);
         
           
            return true;          
        }
        private string PopulateBody(string email, string password,string fullName)
        {
            string body = string.Empty;

            using (StreamReader reader = new StreamReader("Email/ClientCredential.tpl"))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{{user_name}}", email);
            body = body.Replace("{{pass_word}}", password);
            body = body.Replace("{{full_name}}", fullName);
            return body;
        }

        private async Task SendMailAsync(string email, string password,string fullname)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("99hungthinh17.2019@gmail.com");

                mail.To.Add(email);
                mail.Subject = "Thông tin đăng nhập";
                mail.Body = PopulateBody(email,password,fullname);
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.Normal;
               
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential("99hungthinh17.2021", "Koieuai1712@");
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 30000;
                    await smtpClient.SendMailAsync(mail);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string RandomPassword()
        {

            var s = new char[6];
            for (var i = 0; i < s.Length; i++)
                s[i] = '0';

            s[0] = '1';

            s[1] = '2';

            s[2] = '3';

            s[3] = '4';


            Random random = new Random();

            for (var i = 0; i < s.Length; i++)
            {
                var i1 = random.Next(0, s.Length);
                var i2 = random.Next(0, s.Length);

                var temp = s[i1];
                s[i1] = s[i2];
                s[i2] = temp;
            }

            StringBuilder password = new StringBuilder();
            char c;

            for (var i = 0; i < s.Length; i++)
            {
                c = s[i] switch
                {
                    '0' => (char)random.Next(33, 126),
                    '1' => (char)random.Next(33, 48),
                    '2' => (char)random.Next(48, 58),
                    '3' => (char)random.Next(97, 123),
                    '4' => (char)random.Next(65, 91),
                    _ => (char)0,
                };

                if (c != 0)
                    password.Append(c);
            }

            return password.ToString();
        }

    }
}
