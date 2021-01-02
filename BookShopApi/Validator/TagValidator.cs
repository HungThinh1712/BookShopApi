using BookShopApi.Models;
using FluentValidation;


namespace BookShopApi.Validator
{
    public class TagValidator : AbstractValidator<Tag>
    {
        public TagValidator()
        {
            RuleFor(tag => tag.Name).NotEmpty().WithMessage("Vui lòng nhập thẻ");
        }
    }
}
