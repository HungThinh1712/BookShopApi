using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Auth;
using BookShopApi.Models.ViewModels.Users;
using BookShopApi.Service;
using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Linq;

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
        private readonly OrderService _orderService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public UsersController(UserService userService,
            ShoppingCartService shoppingCartService,
            IBackgroundTaskQueue queueService,
            ProvinceService provinceService,
            DistrictService districtService,
            WardService wardService,
            OrderService orderService,
             IWebHostEnvironment hostEnvironment)
        {
            _userService = userService;
            _shoppingCartService = shoppingCartService;
            _queueService = queueService;
            _districtService = districtService;
            _provinceService = provinceService;
            _wardService = wardService;
            _orderService = orderService;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var user = await _userService.GetAllAsync();
            return Ok(user);
        }
        [HttpPost("Admin")]
        public async Task<ActionResult> CreateAdmin(AdminRM admin)
        {
            await _userService.CreateAdminsAsync(admin);
            return Ok(true);
        }
        [HttpPost("Admin/Update")]
        public async Task<ActionResult> UpdateAdmin(UpdatedcAdmin admin)
        {
            await _userService.UpdateAdminAsync(admin);
            return Ok(true);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<UserViewModel>> Get(
            [FromQuery(Name = "id")] string id
            )
        {
            var user = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            user.ImgUrl = user.ImgUrl;
            if (user.IsAdmin == false)
            {
                user.BirthDay = Convert.ToDateTime(user.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
            }
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
            user.ImgUrl = "https://www.pphfoundation.ca/wp-content/uploads/2018/05/default-avatar.png";
            _queueService.QueueBackgroundWorkItem(async token =>
            {
                await SendMailAsync(user.Email, user.CodeActive,"Xác thực tài khoản");
                await Task.Delay(60000);
                await _userService.UpdateCodeActiveAsync(user.Id);

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

        
            string address = user["address"].ToString();
           
         
          
          
            string name = user["name"].ToString();
            string phone = user["phone"].ToString();

            string id = user["id"].ToString();
            string provinceId = user["province"].ToString();
            string districtId = user["district"].ToString();
            string wardId = user["ward"].ToString();
            var tempUser = await _userService.GetAsync(id);
            //Date locations
            var province = await _provinceService.GetByIdAsync(provinceId);
            var district = await _districtService.GetByIdAsync(districtId);
            var ward = await _wardService.GetByIdAsync(wardId);

            address = string.Format("{0}, {1}, {2}, {3}", address, ward.Name, district.Name, province.Name);

            await _userService.UpdateAddressAsync(id, name, phone, address,provinceId,districtId,wardId);

            ///Return updated user to update state in react
            var returnedUser = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
           
          
            returnedUser.ImgUrl = returnedUser.ImgUrl;
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
        private async Task SendMailAsync(string email, string code,string title)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("99hungthinh17.2019@gmail.com");

                mail.To.Add(email);
                mail.Subject = title;
                mail.Body = string.Format("Mã xác thực là: {0}",code);
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
        
            returnedUser.ImgUrl = returnedUser.ImgUrl;
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
            if (oldPassword == newPassword)
                return BadRequest("Mật khẩu mới không được trùng mật khẩu cũ");
            //Check password
            if (await _userService.CheckPassword(id, oldPassword) == false)
                return BadRequest("Mật khẩu không đúng");

            string name = user["name"].ToString();
            string phone = user["phone"].ToString();
            int sex = Convert.ToInt32(user["sex"].ToString());



            DateTime birthday = Convert.ToDateTime(user["birthday"].ToString());
            await _userService.UpdateProfileWithPassWordAsync(id, name, phone, sex, birthday, newPassword);
            //return user updated
            var returnedUser = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            returnedUser.ImgUrl = returnedUser.ImgUrl;
            returnedUser.BirthDay = Convert.ToDateTime(returnedUser.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
            return Ok(returnedUser);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> SendCodeResetPassWord([FromQuery] string email)
        {
            //Hash password
            var user = await _userService.GetUserbyEmailAsync(email);
            if (user == null)
                return BadRequest("Email không tồn tại");
            user.CodeResetPassWord = RandomCode();
            _queueService.QueueBackgroundWorkItem(async token =>
            {
                await SendMailAsync(email, user.CodeResetPassWord,"Thay đổi mật khẩu");
                await Task.Delay(60000);
                await _userService.UpdateCodePasswordAsync(user.Id);

            });
            await _userService.UpdateAsync(user.Id, user);
            return Ok("Một mã xác nhận đã được gửi tới email của bạn");

        }

        [HttpPut("[action]")]
        public async Task<ActionResult> ConfirmCodeResetPassWord(ConfirmModel confirm)
        {

            var user = await _userService.GetUserLoginbyEmailAsync(confirm.Email);
            if (user.CodeResetPassWord == confirm.Code)
                return Ok("Xác thực thành công");
            else
            {
                return BadRequest("Mã xác nhận không đúng hoặc đã hết hạn");
            }

        }
        [HttpPut("[action]")]
        public async Task<ActionResult> ChangePassword(ChangePassWordModel changePassWordModel)
        {

            var user = await _userService.GetUserbyEmailAsync(changePassWordModel.Email);
            user.PassWord = BCrypt.Net.BCrypt.HashPassword(changePassWordModel.Password);
            await _userService.UpdateAsync(user.Id, user);
            return Ok("Cập nhật thành công");

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
        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateAvatarUser([FromForm] UpdateAvatarModel updatedUser)
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var user = await _userService.GetAsync(userId);
            updatedUser.Id = userId;


            await _userService.UpdateAvatarAsync(updatedUser);

            ///Return updated user to update state in react
            var returnedUser = (await _userService.GetAsync(userId)).Adapt<UserViewModel>();
           
            returnedUser.ImgUrl = returnedUser.ImgUrl;
            returnedUser.BirthDay = Convert.ToDateTime(returnedUser.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
            return Ok(returnedUser);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ResendConfirmCode([FromQuery] string email)
        {
            //Hash password
            var user = await _userService.GetUserLoginbyEmailAsync(email);
            if (user == null)
                return BadRequest("Email không tồn tại");
            user.CodeActive = RandomCode();
            _queueService.QueueBackgroundWorkItem(async token =>
            {
                await SendMailAsync(email, user.CodeActive,"Xác thực tài khoản");
                await Task.Delay(60000);
                await _userService.UpdateCodeActiveAsync(user.Id);

            });
            await _userService.UpdateAsync(user.Id, user);
            return Ok("Một mã xác nhận đã được gửi tới email của bạn");

        }



        [HttpGet("Admin/[action]")]
        public async Task<ActionResult<IEnumerable<User>>> GetUser([FromQuery] string name, int page)
        {
            var users = await _userService.GetAllAsync(name);
            foreach (var user in users.Entities)
            {
                user.BirthDay = Convert.ToDateTime(user.BirthDay).ToLocalTime().ToString("dd-MM-yyyy");

                //user.SumOrder = await GetSumOrder(user.Id);
            }
            return Ok(users);
        }
        [HttpGet("Admin/[action]")]
        public async Task<ActionResult<IEnumerable<User>>> GetAdminsUser([FromQuery] string name)
        {
            var users = await _userService.GetAdminsAsync(name);
            
            return Ok(users);
        }
        [HttpGet("Admin/[action]")]
        public async Task<ActionResult> Activate([FromQuery] string id,bool isActivate)
        {
            return Ok(await _userService.DeactivateOrActivateAdmin(id, isActivate));
        }




        private async Task<int> GetSumOrder(string id)
        {
            return (await _orderService.GetOrdersAsync(id)).Count;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CreateUserFacebook(string email, string fullName)
        {
            //Hash password        
            var user = await _userService.GetUserByEmailAsync(email);
            if (user != null)
            {
                return Ok(Authenticate.GetToken(user.Id, user.IsAdmin));
            }

            else
            {
                var createdUser = (await _userService.CreateAsync(email, fullName)).Adapt<UserViewModel>();
                //Create shopping Cart each time create user
                ShoppingCart shoppingCart = new ShoppingCart
                {
                    UserId = createdUser.Id
                };

                await _shoppingCartService.CreateAsync(shoppingCart);
                return Ok(Authenticate.GetToken(createdUser.Id, createdUser.IsAdmin));
            }
        }
        
    }
}