using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Users
{
    public class UsersInAdminViewModel
    {
      
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string SpecificAddress { get; set; }
        public int Sex { get; set; }
        public string BirthDay { get; set; }
        public int SumOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
