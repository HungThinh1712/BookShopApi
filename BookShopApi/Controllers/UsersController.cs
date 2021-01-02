using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Users;
using BookShopApi.Service;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ShoppingCartService _shoppingCartService;
        private readonly IBackgroundTaskQueue _queueService;
        private readonly ProvinceService _provinceService;
        private readonly DistrictService _districtService;
        private readonly WardService _wardService;

        public UsersController(UserService userService, 
            ShoppingCartService shoppingCartService,
            IBackgroundTaskQueue queueService,
            ProvinceService provinceService,
            DistrictService districtService,
            WardService wardService)
        {
            _userService = userService;
            _shoppingCartService = shoppingCartService;
            _queueService = queueService;
            _districtService = districtService;
            _provinceService = provinceService;
            _wardService = wardService;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var user = await _userService.GetAsync();
            return Ok(user);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<UserViewModel>> Get(
            [FromQuery(Name = "id")] string id
            )
        {
            var user = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            if(user.ProvinceId !=null && user.ProvinceId != "")
            {
                user.ProvinceName = (await _provinceService.GetByIdAsync(user.ProvinceId)).Name;
                user.DistrictName = (await _districtService.GetByIdAsync(user.DistrictId)).Name;
                user.WardName = (await _wardService.GetByIdAsync(user.WardId)).Name;
            }
            user.BirthDay = Convert.ToDateTime(user.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
            if (user == null)
            {
                return BadRequest("user not found");
            }
            return Ok(user);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create(User user)
        {
            //Hash password
            user.PassWord = BCrypt.Net.BCrypt.HashPassword(user.PassWord);
            user.IsActive = false;
            user.CodeActive = RandomCode();
            _queueService.QueueBackgroundWorkItem(async token  =>
            {
                await SendMailAsync(user.Email, user.CodeActive);
               
            });
       
            var createdUser = (await _userService.CreateAsync(user)).Adapt<UserViewModel>();
            createdUser.BirthDay = Convert.ToDateTime(createdUser.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
            //Create shopping Cart each time create user
            ShoppingCart shoppingCart = new ShoppingCart
            {
                UserId = createdUser.Id
            };
           
            await _shoppingCartService.CreateAsync(shoppingCart);
            return Ok(createdUser);

        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateAddress(JObject updatedUser)
        {
            var user = updatedUser["updatedUser"];

            string province = user["provinceId"].ToString();
            string district = user["districtId"].ToString();
            string ward = user["wardId"].ToString();
            string address = user["address"].ToString();
            if (province == "0" || ward == "0" || district == "0" || address == "")
                return BadRequest("Vui lòng nhập đầy đủ thông tin");

            string name = user["name"].ToString();
            string phone = user["phone"].ToString();
           
            string id = user["id"].ToString();
            await _userService.UpdateAddressAsync(id, name,phone ,province,district,ward,address);

            ///Return updated user to update state in react
            var returnedUser = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            returnedUser.ProvinceName = (await _provinceService.GetByIdAsync(returnedUser.ProvinceId)).Name;
            returnedUser.DistrictName = (await _districtService.GetByIdAsync(returnedUser.DistrictId)).Name;
            returnedUser.WardName = (await _wardService.GetByIdAsync(returnedUser.WardId)).Name;
            returnedUser.BirthDay = Convert.ToDateTime(returnedUser.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
            return Ok(returnedUser);
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userService.GetAsync(id);
            user.DeleteAt = DateTime.UtcNow;
            await _userService.UpdateAsync(id, user);
            return Ok("Delete sucessfully");
        }
        private async Task SendMailAsync(string email, string code)
        {

            HttpContext.Session.Set("Code", code);
            MailMessage mail = new MailMessage();


            mail.From = new MailAddress("99hungthinh17.2019@gmail.com");

            mail.To.Add(email);
            mail.Subject = "Xác nhận bằng mã xác thực";
            mail.Body = HttpContext.Session.Get<string>("Code");
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.Normal;
            mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential("99hungthinh17.2019", "Koieuai1712");
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = true;
                smtpClient.Timeout = 30000;
                await smtpClient.SendMailAsync(mail);
            }             
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateProfileUser(JObject updatedUser)
        {
            var user = updatedUser["updatedUser"];
            string name = user["name"].ToString();
            string phone = user["phone"].ToString();
            int sex = Convert.ToInt32(user["sex"].ToString());
            string id = user["id"].ToString();
            DateTime birthday = Convert.ToDateTime(user["birthday"].ToString());
            await _userService.UpdateProfileAsync(id, name, phone, sex, birthday);
            //return user updated
            var returnedUser = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            returnedUser.ProvinceName = (await _provinceService.GetByIdAsync(returnedUser.ProvinceId)).Name;
            returnedUser.DistrictName = (await _districtService.GetByIdAsync(returnedUser.DistrictId)).Name;
            returnedUser.WardName = (await _wardService.GetByIdAsync(returnedUser.WardId)).Name;
            returnedUser.BirthDay = Convert.ToDateTime(returnedUser.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");   
            return Ok(returnedUser);
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateProfileUserWithPassWord(JObject updatedUser)
        {

            var user = updatedUser["updatedUser"];
            string id = user["id"].ToString();
            string oldPassword = user["oldPassword"].ToString();
            string newPassword = user["newPassword"].ToString();
            if(oldPassword == newPassword)
                return BadRequest("Mật khẩu mới không được trùng mật khẩu cũ");
            //Check password
            if (await _userService.CheckPassword(id, oldPassword) == false)
                return BadRequest("Mật khẩu không đúng");

            string name = user["name"].ToString();
            string phone = user["phone"].ToString();
            int sex = Convert.ToInt32(user["sex"].ToString());
           
           
           
            DateTime birthday = Convert.ToDateTime(user["birthday"].ToString());
            await _userService.UpdateProfileWithPassWordAsync(id, name, phone, sex, birthday,newPassword);
            //return user updated
            var returnedUser = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            returnedUser.BirthDay = Convert.ToDateTime(returnedUser.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
            return Ok(returnedUser);
        }
        private string RandomCode()
        {
            string str = "0123456789";
            string code = "";
            Random rnd = new Random();
            for (int i = 0; i < 6; i++)
            {
                int p = rnd.Next(0, 10);
                code += str[p];
            }
            return code;
        }

        
    }
}