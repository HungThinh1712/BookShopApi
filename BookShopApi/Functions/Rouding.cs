using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Functions
{
    public static class Rouding
    {
        public static double Adjust(double input)
        {
            double whole = Math.Truncate(input);
            double remainder = input - whole;
            if (remainder < 0.3)
            {
                remainder = 0;
            }
            else if (remainder < 0.8)
            {
                remainder = 0.5;
            }
            else
            {
                remainder = 1;
            }
            return whole + remainder;
        }
    }
}
