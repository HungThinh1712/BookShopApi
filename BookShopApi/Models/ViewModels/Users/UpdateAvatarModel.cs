using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Users
{
    public class UpdateAvatarModel
    {
        public string Id { get; set; }
        public IFormFile ImageFile { get; set; }
        public string ImageName { get; set; }
    }
}
