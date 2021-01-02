using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models
{
    public class Ward
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DistrictId { get; set; }
        public string Alias { get; set; }
    }
}
