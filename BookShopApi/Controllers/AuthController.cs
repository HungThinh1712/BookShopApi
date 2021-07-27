using BookShopApi.Functions;
using BookShopApi.Models.ViewModels.Auth;
using BookShopApi.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Mail;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp;

namespace BookShopApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {
            
            var user = await _userService.GetUserLoginbyEmailAsync(loginViewModel.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginViewModel.PassWord, user.PassWord))
                return BadRequest("Email hoặc mật khẩu không đúng!");
            if (user!=null &&  user.IsActive == false)
            {
                return BadRequest("Email chưa được xác nhận");
            }

          
           
            return Ok(Authenticate.GetToken(user.Id, user.IsAdmin));
        }

        [HttpPut]
        public async Task<ActionResult> ConfirmCode(ConfirmModel confirm)
        {
           
            var user = await _userService.GetUserLoginbyEmailAsync(confirm.Email);
            if (user.CodeActive == confirm.Code)
            {
                user.IsActive = true;
                user.CodeActive = null;
                await _userService.UpdateAsync(user.Id, user);
                return Ok(Authenticate.GetToken(user.Id, user.IsAdmin));
            }
            else
            {
                return BadRequest("Mã xác nhận không đúng");
            }
           
        }


    }
}
