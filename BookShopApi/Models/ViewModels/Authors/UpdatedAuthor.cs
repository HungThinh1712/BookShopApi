using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Authors
{
    public class UpdatedAuthor
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public DateTime BirthDay { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
    }
}
