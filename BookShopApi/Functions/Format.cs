using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Functions
{
    public static class Format
    {
        public static string FormatNumberString(string s)
        {
            return (float.Parse(s) / 1000).ToString(".#00");
        }
    }
}
