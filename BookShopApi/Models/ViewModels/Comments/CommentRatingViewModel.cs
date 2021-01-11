using BookShopApi.Models.ViewModels.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Comments
{
    public class CommentRatingViewModel
    {
        public List<RatingViewModel> Ratings { get; set; }
        public EntityList<CommentViewModel> Comments { get; set; }
    }
}
