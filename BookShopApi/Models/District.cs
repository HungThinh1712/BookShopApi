using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models
{
    public class District
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProvinceId { get; set; }
        public string Alias { get; set; }
    }
}
