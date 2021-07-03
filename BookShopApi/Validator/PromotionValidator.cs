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
            //}).WithMessage("Mã khuyến mãi đã tồn tại");
            RuleFor(p => p.StartDate.Date).GreaterThan(DateTime.UtcNow.AddHours(7).Date).WithMessage("Ngày bắt đầu phải ở tương lai");
            RuleFor(p => p.EndDate.Date).GreaterThan(p => p.StartDate.Date).WithMessage("Ngày kết thúc phải lớn hơn ngày bắt đầu");
        }
    }
}
