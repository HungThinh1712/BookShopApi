using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Promotion
{
    public class PromotionViewModel
    {
        public PromotionType PromotionType { get; set; }
        public decimal DiscountMoney { get; set; }
    }
}
