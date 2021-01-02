using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models
{
    public class EntityList<T>
    {
        
        public int Total { get; set; }
        public List<T> Entities { get; set; }
    }
}
