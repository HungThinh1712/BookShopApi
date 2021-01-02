using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Functions
{
    public static class ConverDateTimeToString
    {
        public static string ConvertDateTime(DateTime dateTime)
        {
            string date = Convert.ToDateTime(dateTime).ToString("dd-MM-yyyy");
            return date;
        }
    }
}
