using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Books;
using BookShopApi.Models.ViewModels.Comments;
using BookShopApi.Service;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly BookService _bookService;
      

        public CommentsController(CommentService commentService, BookService bookService)
        {
            _commentService = commentService;
            _bookService = bookService;
            
        }

        [HttpGet]
        public async Task<ActionResult<List<CommentViewModel>>> GetComments(
            [FromQuery] string bookId)
        {
            var comments = await _commentService.GetAsync(bookId);
            return Ok(comments);
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<List<RatingViewModel>>> GetRatings(
            [FromQuery] string bookId)
        {
            var ratings = await _commentService.GetAmountRating(bookId);
            return Ok(ratings);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> Create(Comment comment)
        {
            var createdComment = await _commentService.CreateAsync(comment);
            var bookRated = await _bookService.GetAsync(comment.BookId);
            if (bookRated.Rating == null)
                bookRated.Rating = new List<int>();
            bookRated.Rating.Add(comment.Rate);
            await _bookService.UpdateAsync(bookRated.Id, bookRated);
            //Return comments and ratings
            var commnets = await _commentService.GetAsync(bookRated.Id);
            var ratings = await _commentService.GetAmountRating(bookRated.Id);

            var commentWithRating = new CommentRatingViewModel()
            {
                Comments = commnets,
                Ratings = ratings
            };
            return Ok(commentWithRating);
        }
        
    }
}
