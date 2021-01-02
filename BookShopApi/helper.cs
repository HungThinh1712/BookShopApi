using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi
{
    public class helper
    {
        public static Dictionary<string, long> build_meta_paging(dynamic entityPagination)
        {
            Dictionary<string, long> data = new Dictionary<string, long>();
            data.Add("total", entityPagination.Total);
            data.Add("count", entityPagination.Count);
            data.Add("per_page", entityPagination.PerPage);
            data.Add("current_page", entityPagination.CurrentPage);
            data.Add("total_pages", (long)Math.Ceiling(Convert.ToDecimal(entityPagination.Total) / Convert.ToDecimal(entityPagination.PerPage)));
            return data;
        }
    }
}
