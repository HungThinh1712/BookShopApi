using BookShopApi.Models;
using FluentValidation;


namespace BookShopApi.Validator
{
   
    public class BookValidator : AbstractValidator<Book>
    {
        public BookValidator()
        {
           
            RuleFor(book => book.Description).NotEmpty().WithMessage("Vui lòng nhập mô tả");
            RuleFor(book => book.ImageFile).NotNull().WithMessage("Vui lòng chọn hình ảnh");
            RuleFor(book => book.BookName).NotEmpty().WithMessage("Vui lòng nhập tên sách");
            RuleFor(book => book.ZoneType).NotEqual("0").WithMessage("Vui lòng chọn khu vực");
            RuleFor(book => book.PublishHouseId).NotEqual("0").WithMessage("Vui lòng chọn nhà xuất bản");
            RuleFor(book => book.TypeId).NotEqual("0").WithMessage("Vui lòng chọn thể loại");
            RuleFor(book => book.AuthorId).NotEqual("0").WithMessage("Vui lòng chọn tác giả");
            RuleFor(book => book.Amount).NotEmpty().WithMessage("Vui lòng nhập số lượng");
            RuleFor(book => book.Price).NotEmpty().WithMessage("Vui lòng nhập giá sản phẩm");
            RuleFor(book => book.CoverPrice).NotEmpty().WithMessage("Vui lòng nhập giá bìa");
            RuleFor(book => book.PageAmount).NotEmpty().WithMessage("Vui lòng nhập số trang");
            RuleFor(book => book.Size).NotEmpty().WithMessage("Vui lòng nhập kích thước");
            RuleFor(book => book.CoverType).NotEmpty().WithMessage("Vui lòng nhập loại bìa");
        }
    }
}
