using BookShopApi.Models;
using BookShopApi.Service;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Validator
{
   
    public class UserValidator : AbstractValidator<User>
    {
        private readonly UserService _userService;
        public UserValidator(UserService userService)
        {
            _userService = userService;
            RuleFor(user => user.Email).EmailAddress().WithMessage("Email không hợp lệ").MustAsync(async (email, cancellation) => {
                bool exists = await _userService.GetAsyncByEmail(email);
                return !exists;
            }).WithMessage("Email đã đăng ký tài khoản");
            RuleFor(user => user.PassWord).NotEmpty().WithMessage("Mật khẩu không hợp lệ").MaximumLength(20).WithMessage("Mật khẩu không hợp lệ");
            RuleFor(user => user.Phone).Length(10).WithMessage("Số điện thoại không hợp lệ").MustAsync(async (phone, cancellation) => {
                bool exists = await _userService.GetAsyncByPhone(phone);
                return !exists;
            }).WithMessage("Số điện thoại đã đăng ký tài khoản");

        }
    }
}
