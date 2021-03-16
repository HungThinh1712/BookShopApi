using BookShopApi.Models;
using FluentValidation;


namespace BookShopApi.Validator
{
    public class TagValidator : AbstractValidator<Book>
    {
        public TagValidator()
        {
            RuleFor(book => book.Tag).NotEmpty().WithMessage("Vui lòng nhập thẻ");
        }
    }
}
