using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Functions
{
    public static class Average
    {
        public static double CountingAverage(List<int> arr)
        {
            if (arr == null || arr.Count == 0)
            {
                return 0;
            }
            else
            {
                int sum = 0;
                foreach (int i in arr)
                {
                    sum += i;
                }
                return sum / (double)arr.Count;

            }

        }

        
    }
}
