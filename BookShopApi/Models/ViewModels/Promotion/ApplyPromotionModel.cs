using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Promotion
{
    public class ApplyPromotionModel
    {
        public string PromotionCode { get; set; }
        public decimal TotalMoney { get; set; }
        public List<string> BookIds { get; set; }
    }
}
