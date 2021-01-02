using FluentValidation;
using BookShopApi.Models;

namespace BookShopApi.Validator
{
    public class TypeValidator : AbstractValidator<BookType>
    {
        public TypeValidator()
        {
            RuleFor(type => type.Name).NotNull().NotEmpty();
        }
    }
}
