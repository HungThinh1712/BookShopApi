using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Auth;
using BookShopApi.Models.ViewModels.Users;
using BookShopApi.Service;
using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            var user = await _userService.GetAsync();
            return Ok(user);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<UserViewModel>> Get(
            [FromQuery(Name = "id")] string id
            )
        {
            var user = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            user.ImgSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, user.ImageName);
            if (user.ProvinceId != null && user.ProvinceId != "" &&user.IsAdmin==false)
            {
                user.ProvinceName = (await _provinceService.GetByIdAsync(user.ProvinceId)).Name;
                user.DistrictName = (await _districtService.GetByIdAsync(user.DistrictId)).Name;
                user.WardName = (await _wardService.GetByIdAsync(user.WardId)).Name;
            }
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
            user.ImageName = "defaultAvatar.png";
            _queueService.QueueBackgroundWorkItem(async token =>
            {
                await SendMailAsync(user.Email, user.CodeActive);
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

            string province = user["provinceId"].ToString();
            string district = user["districtId"].ToString();
            string ward = user["wardId"].ToString();
            string address = user["address"].ToString();
            if (province == "0" || ward == "0" || district == "0" || address == "")
                return BadRequest("Vui lòng nhập đầy đủ thông tin");

            string name = user["name"].ToString();
            string phone = user["phone"].ToString();

            string id = user["id"].ToString();
            var tempUser = await _userService.GetAsync(id);            

            await _userService.UpdateAddressAsync(id, name, phone, province, district, ward, address);

            ///Return updated user to update state in react
            var returnedUser = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            if (returnedUser.ProvinceId != null)
            {
                returnedUser.ProvinceName = (await _provinceService.GetByIdAsync(returnedUser.ProvinceId)).Name;
                returnedUser.DistrictName = (await _districtService.GetByIdAsync(returnedUser.DistrictId)).Name;
                returnedUser.WardName = (await _wardService.GetByIdAsync(returnedUser.WardId)).Name;
            }
            returnedUser.ImgSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, returnedUser.ImageName);
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
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("99hungthinh17.2019@gmail.com");

            mail.To.Add(email);
            mail.Subject = "Xác nhận bằng mã xác thực";
            mail.Body = code;
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
            if(returnedUser.ProvinceId!=null && returnedUser.WardId!=null && returnedUser.DistrictId != null)
            {
                returnedUser.ProvinceName = (await _provinceService.GetByIdAsync(returnedUser.ProvinceId)).Name;
                returnedUser.DistrictName = (await _districtService.GetByIdAsync(returnedUser.DistrictId)).Name;
                returnedUser.WardName = (await _wardService.GetByIdAsync(returnedUser.WardId)).Name;
            }
            returnedUser.ImgSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, returnedUser.ImageName);
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
            returnedUser.ImgSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, returnedUser.ImageName);
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
                await SendMailAsync(email, user.CodeResetPassWord);
                await Task.Delay(60000);
                await _userService.UpdateCodePasswordAsync(user.Id);

            });
            await _userService.UpdateAsync(user.Id, user);
            return Ok("Một mã xác nhận đã được gửi tới email của bạn");

        }

        [HttpPut("[action]")]
        public async Task<ActionResult> ConfirmCodeResetPassWord(ConfirmModel confirm)
        {

            var user = await _userService.GetUserbyEmailAsync(confirm.Email);
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
        public async Task<IActionResult> UpdateAvatarUser([FromForm] UpdateAvatarModel updatedUser )
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var user = await _userService.GetAsync(userId);
            updatedUser.ImageName = user.ImageName;
            updatedUser.Id = userId;
            if (updatedUser.ImageFile != null)
            {
                if(user.ImageName!= "defaultAvatar.png")
                    DeleteImage(user.ImageName);
                updatedUser.ImageName = await SaveImageAsync(updatedUser.ImageFile);
            }

            await _userService.UpdateAvatarAsync(updatedUser);

            ///Return updated user to update state in react
            var returnedUser = (await _userService.GetAsync(userId)).Adapt<UserViewModel>();
            if (returnedUser.ProvinceId != null)
            {
                returnedUser.ProvinceName = (await _provinceService.GetByIdAsync(returnedUser.ProvinceId)).Name;
                returnedUser.DistrictName = (await _districtService.GetByIdAsync(returnedUser.DistrictId)).Name;
                returnedUser.WardName = (await _wardService.GetByIdAsync(returnedUser.WardId)).Name;
            }
            returnedUser.ImgSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, returnedUser.ImageName);
            returnedUser.BirthDay = Convert.ToDateTime(returnedUser.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
            return Ok(returnedUser);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ResendConfirmCode([FromQuery] string email)
        {
            //Hash password
            var user = await _userService.GetUserbyEmailAsync(email);
            if (user == null)
                return BadRequest("Email không tồn tại");
            user.CodeActive = RandomCode();
            _queueService.QueueBackgroundWorkItem(async token =>
            {
                await SendMailAsync(email, user.CodeActive);
                await Task.Delay(60000);
                await _userService.UpdateCodeActiveAsync(user.Id);

            });
            await _userService.UpdateAsync(user.Id, user);
            return Ok("Một mã xác nhận đã được gửi tới email của bạn");

        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string imageName = new string(Path.GetFileNameWithoutExtension(imageFile.FileName).Take(10).ToArray()).Replace(' ', '-');
            imageName = imageName + DateTime.Now.ToString("yymmssff") + Path.GetExtension(imageFile.FileName);
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return imageName;
        }
        private void DeleteImage(string imageName)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }

        [HttpGet("Admin/[action]")]
        public async Task<ActionResult<IEnumerable<User>>> GetUser([FromQuery] string name,int page)
        {
            var users = await _userService.GetAllAsync(name);
            foreach(var user in users.Entities)
            {
                user.BirthDay = Convert.ToDateTime(user.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
                user.SumOrder = await GetSumOrder(user.Id);
                user.Address = await GetAddress(user.Id);
            }
            return Ok(users);
        }

        [HttpGet("Admin/[action]")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUser([FromQuery] string name, int page)
        {
            var users = await _userService.GetAllUserAsync(name);
            foreach (var user in users.Entities)
            {
                user.BirthDay = Convert.ToDateTime(user.BirthDay).ToLocalTime().ToString("yyyy-MM-dd");
                user.Address = await GetAddressUser(user.Id);
            }
            return Ok(users);
        }

        private async Task<string> GetAddress(string id)
        {
            var returnedUser = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            if (returnedUser.ProvinceId != null)
            {
                returnedUser.ProvinceName = (await _provinceService.GetByIdAsync(returnedUser.ProvinceId)).Name;
                returnedUser.DistrictName = (await _districtService.GetByIdAsync(returnedUser.DistrictId)).Name;
                returnedUser.WardName = (await _wardService.GetByIdAsync(returnedUser.WardId)).Name;
                return returnedUser.SpecificAddress + ", " + returnedUser.WardName + ", " + returnedUser.DistrictName + ", " + returnedUser.ProvinceName;
            }
            else
            {
                return "Chưa cập nhật địa chỉ giao hàng";
            }
        }

        private async Task<string> GetAddressUser(string id)
        {
            var returnedUser = (await _userService.GetAsync(id)).Adapt<UserViewModel>();
            if (returnedUser.ProvinceId != null)
            {
                returnedUser.ProvinceName = (await _provinceService.GetByIdAsync(returnedUser.ProvinceId)).Name;
                returnedUser.DistrictName = (await _districtService.GetByIdAsync(returnedUser.DistrictId)).Name;
                returnedUser.WardName = (await _wardService.GetByIdAsync(returnedUser.WardId)).Name;
                return returnedUser.SpecificAddress + ", " + returnedUser.WardName + ", " + returnedUser.DistrictName + ", " + returnedUser.ProvinceName;
            }
            else
            {
                return "Tài khoản chưa cập nhật địa chỉ";
            }
        }

        private async Task<int> GetSumOrder(string id)
        {
            return (await _orderService.GetAsync(id)).Total;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CreateUserFacebook(string email,string fullName)
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