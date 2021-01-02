using BookShopApi.Models.ViewModels.Auth;
using FluentValidation;


namespace BookShopApi.Validator
{
    public class LoginValidator : AbstractValidator<LoginViewModel>
    {
        public LoginValidator()
        {
            RuleFor(login=>login.Email).EmailAddress().WithMessage("Email không hợp lệ").
                NotEmpty().WithMessage("Vui lòng nhập email");
            RuleFor(login => login.PassWord).NotEmpty().WithMessage("Vui lòng nhập mật khẩu");
        }
    }
}
