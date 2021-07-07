using BookShopApi.Models.ViewModels.Users;
using BookShopApi.Service;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Validator
{
  
    public class AdminValidator : AbstractValidator<AdminRM>
    {
        private readonly UserService _userService;
        public AdminValidator(UserService userService)
        {
            _userService = userService;
            RuleFor(user => user.Email).EmailAddress().WithMessage("Email không hợp lệ").MustAsync(async (email, cancellation) => {
                bool exists = await _userService.GetAsyncByEmail(email);
                return !exists;
            }).WithMessage("Email đã đăng ký tài khoản");
            

        }
    }
}
