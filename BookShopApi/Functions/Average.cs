using BookShopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Functions
{
    public static class Average
    {
        public static double CountingAverage(List<Comment> arr)
        {
            if (arr == null || arr.Count == 0)
            {
                return 0;
            }
            else
            {
                int sum = arr.Select(x => x.Rate).Sum();
                return sum / (double)arr.Count;

            }

        }

        
    }
}
