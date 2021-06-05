using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels
{
    public class ItemInCartViewModel
    {
        public string BookId { get; set; }
        public int Amount { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string CoverPrice { get; set; }
        public string AuthorName { get; set; }
        public string ImageSrc { get; set; }
        public bool StatusRate { get; set; }

    }
}
