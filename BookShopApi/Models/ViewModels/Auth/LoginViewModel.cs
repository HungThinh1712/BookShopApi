using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Auth
{
    public class LoginViewModel
    {
        public string Email { get; set; }
        public string PassWord { get; set; }
    }
}
