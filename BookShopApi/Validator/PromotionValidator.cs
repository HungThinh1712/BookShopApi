using BookShopApi.Models;
using BookShopApi.Service;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Validator
{
   
    public class PromotionValidator : AbstractValidator<Promotion>
    {
        private readonly PromotionService _promotionService;
        public PromotionValidator(PromotionService promotionService)
        {
            _promotionService = promotionService;
            //RuleFor(p => p.PromotionCode).MustAsync(async (promotion, cancellation) => {
            //    bool exists = await _promotionService.CheckPromotionAsync(promotion);
            //    return !exists;
            //}).WithMessage("Mã khuyến mãi đã tồn tại");
            RuleFor(p => p.StartDate).GreaterThan(DateTime.UtcNow.Date.AddDays(1)).WithMessage("Ngày bắt đầu phải ở tương lai");
            RuleFor(p => p.EndDate.Date).GreaterThan(p => p.StartDate.Date).WithMessage("Ngày kết thúc phải lớn hơn ngày bắt đầu");
        }
    }
}
