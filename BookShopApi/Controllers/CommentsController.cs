using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Books;
using BookShopApi.Models.ViewModels.Comments;
using BookShopApi.Service;
using Mapster;
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
        private readonly UserService _userService;


        public CommentsController(CommentService commentService, BookService bookService,
            UserService userService)
        {
            _commentService = commentService;
            _bookService = bookService;
            _userService = userService;

        }

        [HttpGet]
        public async Task<ActionResult<EntityList<CommentViewModel>>> GetComments(
            [FromQuery] string bookId, int page)
        {
            var comments = await _commentService.GetAsync(bookId, page);
            await GetCommentWithUserName(comments);

            return Ok(comments);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<EntityList<CommentViewModel>>> GetCommentsByUserId(
            [FromQuery] string userId)
        {
            var comments = await _commentService.GetByUserIdAsync(userId);
            if (comments == null)
            {
                return BadRequest("book not found");
            }
            var book = await _bookService.GetAsync(comments.BookId);

            CommentViewModel commentViewModel = new CommentViewModel()
            {
                Id = comments.Id,
                BookName = book.BookName,
                UserId = comments.UserId,
                BookId = comments.BookId,
                Title = comments.Title,
                ImgSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, book.ImageName),
                Rate = comments.Rate,
                Content = comments.Content,
                CreateAt = Convert.ToDateTime(comments.CreateAt).ToString("yyyy-MM-dd")
            };
            return Ok(commentViewModel);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<RatingViewModel>>> GetRatings(
            [FromQuery] string bookId)
        {
            var ratings = await _commentService.GetAmountRating(bookId);
            return Ok(ratings);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> Create(CommentUpdateModel createComment)
        {
            var comment = createComment.Adapt<Comment>();
            var createdComment = await _commentService.CreateAsync(comment);

            var bookRated = await _bookService.GetAsync(comment.BookId);
            bookRated.Comments.Add(comment);

            await _bookService.UpdateAsync(bookRated.Id, bookRated);

            //Return comments and ratings
            var commnets = await _commentService.GetAsync(bookRated.Id);
            await GetCommentWithUserName(commnets);
            var ratings = await _commentService.GetAmountRating(bookRated.Id);

            var commentWithRating = new CommentRatingViewModel()
            {
                Comments = commnets,
                Ratings = ratings
            };
            return Ok(commentWithRating);
        }
        [HttpDelete]
        public async Task<ActionResult<List<CommentViewModel>>> Delete(
            [FromQuery] string id, string bookId)
        {
            await _commentService.RemoveAsync(id);

            var bookRated = await _bookService.GetAsync(bookId);

            var newListComment = await _commentService.GetByIdAsync(bookId);
            bookRated.Comments = newListComment;
            await _bookService.UpdateAsync(bookRated.Id, bookRated);

            var comments = await _commentService.GetAsync(bookId);
            var ratings = await _commentService.GetAmountRating(bookId);
            await GetCommentWithUserName(comments);
            var commentWithRating = new CommentRatingViewModel()
            {
                Comments = comments,
                Ratings = ratings
            };
            return Ok(commentWithRating);
        }
        [HttpPut]
        public async Task<ActionResult<List<CommentViewModel>>> Update(CommentUpdateModel updatedCommentModel)
        {
            var updatedComment = updatedCommentModel.Adapt<Comment>();

            await _commentService.UpdateAsync(updatedComment.Id, updatedComment);

            var bookRated = await _bookService.GetAsync(updatedComment.BookId);

            var newListComment = await _commentService.GetByIdAsync(updatedComment.BookId);
            bookRated.Comments = newListComment;
            await _bookService.UpdateAsync(bookRated.Id, bookRated);

            var comments = await _commentService.GetAsync(updatedComment.BookId, updatedCommentModel.Page);
            var ratings = await _commentService.GetAmountRating(updatedComment.BookId);
            await GetCommentWithUserName(comments);
            var commentWithRating = new CommentRatingViewModel()
            {
                Comments = comments,
                Ratings = ratings
            };
            return Ok(commentWithRating);

        }
        private async Task GetCommentWithUserName(EntityList<CommentViewModel> comments)
        {
            foreach(var comment in comments.Entities)
            {
                var user = await _userService.GetAsync(comment.UserId);
                comment.UserFullName = user.FullName;
                comment.ImgSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, user.ImageName);
            }
        }
    }
}
