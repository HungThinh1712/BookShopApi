﻿using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Books;
using BookShopApi.Models.ViewModels.Comments;
using BookShopApi.Service;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly OrderService _orderService;


        public CommentsController(CommentService commentService, BookService bookService,
            UserService userService, OrderService orderService)
        {
            _commentService = commentService;
            _bookService = bookService;
            _userService = userService;
            _orderService = orderService;

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
        public async Task<ActionResult<EntityList<CommentViewModel>>> GetComents([FromQuery] int page, [FromQuery] int pageSize)
        {

            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var comments = await _commentService.GetAsyncManage(userId, page, pageSize);

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
                ImgUrl = book.ImgUrl,
                Rate = comments.Rate,
                Content = comments.Content,
                CreateAt = Convert.ToDateTime(comments.CreateAt).ToString("yyyy-MM-dd")
            };
            return Ok(commentViewModel);
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<EntityList<CommentViewModel>>> GetComentsByAdmin([FromQuery] int page, [FromQuery] int pageSize)
        {
   
            var comments = await _commentService.GetAsyncManageByAdmin(page, pageSize);
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
        public async Task<IActionResult> Create(CommentUpdateModel createComment)
        {
            var comment = createComment.Adapt<Comment>();
             await _commentService.CreateAsync(comment);

            var bookRated = await _bookService.GetAsync(comment.BookId);
            bookRated.Comments.Add(comment);

            await _bookService.UpdateAsync(bookRated.Id, bookRated);

            //Update status comment in order
            var order =await _orderService.GetOrderAsync(comment.OrderId);
            foreach(var item in order.Items)
            {
                if (item.BookId == comment.BookId)
                {
                    item.StatusRate = true;
                }
            }
            await _orderService.UpdateStatusRateAsync(order);
          
            return Ok();
        }
        [HttpGet("CheckComment")]
        public async Task<ActionResult> CheckComment(string id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            var bookRated = await _bookService.GetAsync(comment.BookId);
            var tempComment = bookRated.Comments.Where(x => x.Id == comment.Id).FirstOrDefault();
            bookRated.Comments.Remove(tempComment);
            comment.IsCheck = true;
            bookRated.Comments.Add(comment);

            await _bookService.UpdateAsync(bookRated.Id, bookRated);

            return Ok(await _commentService.CheckedComment(id));
        }
        private async Task GetCommentWithUserName(EntityList<CommentViewModel> comments)
        {
            foreach(var comment in comments.Entities)
            {
                var user = await _userService.GetAsync(comment.UserId);
                comment.UserFullName = user.FullName;
                comment.ImgUrl = user.ImgUrl;
            }
        }
    }
}
