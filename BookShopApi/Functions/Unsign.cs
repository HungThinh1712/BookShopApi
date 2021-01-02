using System;
using System.Text;
using System.Text.RegularExpressions;


namespace BookShopApi.Functions
{
    public static class Unsign
    {
        public static string convertToUnSign(string s)
        {
            if (s == null || s == "")
                return "";
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
    }
}
