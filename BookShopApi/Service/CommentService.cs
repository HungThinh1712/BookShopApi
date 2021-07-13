using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Books;
using BookShopApi.Models.ViewModels.Comments;
using Mapster;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class CommentService
    {
        private readonly IMongoCollection<Comment> _comments;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Book> _book;

        public CommentService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _comments = database.GetCollection<Comment>(settings.CommentsCollectionName);
            _users = database.GetCollection<User>(settings.UsersCollectionName);
            _book = database.GetCollection<Book>(settings.BooksCollectionName);
        }

        public async Task<EntityList<CommentViewModel>> GetAsyncManage(string userId, int page = 1, int pageSize = 4)
        {
            var query = _comments.Find(comment => comment.UserId == userId && comment.IsCheck ==true);

            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<CommentViewModel>()
            {
                Total = (int)total,
                Entities = await query.Project(x =>
                                     new CommentViewModel
                                     {
                                         Id = x.Id,
                                         CreateAt = Convert.ToDateTime(x.CreateAt).ToString("dd-MM-yyyy"),
                                         Content = x.Content,
                                         Rate = x.Rate,
                                         Title = x.Title,
                                         BookId = x.BookId,
                                         BookName = GetBookName(x.BookId)
                                     }).ToListAsync()
            };
        }
        public async Task<EntityList<CommentViewModel>> GetAsyncManageByAdmin( int page = 1, int pageSize = 12)
        {
            var query = _comments.Find(comment => true);

            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<CommentViewModel>()
            {
                Total = (int)total,
                Entities = await query.Project(x =>
                                     new CommentViewModel
                                     {
                                         Id = x.Id,
                                         CreateAt = Convert.ToDateTime(x.CreateAt).ToString("dd-MM-yyyy"),
                                         Content = x.Content,
                                         Rate = x.Rate,
                                         Title = x.Title,
                                         BookId = x.BookId,
                                         BookName = GetBookName(x.BookId),
                                         IsCheck = x.IsCheck
                                     }).ToListAsync()
            };
        }

        private string GetBookName(string id)
        {
            var books = _book.Find<Book>(book => book.Id == id).FirstOrDefault();
            return books.BookName;
        }

        public async Task<Comment> GetByUserIdAsync(string id) =>
           await _comments.Find<Comment>(comment => comment.UserId == id && comment.IsCheck ==true).FirstOrDefaultAsync();
        public async Task<Comment> GetCommentByIdAsync(string id) =>
         await _comments.Find<Comment>(comment => comment.Id == id).FirstOrDefaultAsync();

        public async Task<List<Comment>> GetByIdAsync(string id) =>
            await _comments.Find(comment => comment.BookId ==id && comment.IsCheck ==true).ToListAsync();
        //public async Task<List<Comment>> GetByUserIdAsync(string id) =>
        //    await _comments.Find(comment => comment.UserId == id).ToListAsync();


        public async Task<EntityList<CommentViewModel>> GetAsync(string id, int page = 1)
        {
            if (page == 0)
                page = 1;

            var query = _comments.Find(x => x.BookId == id && x.IsCheck ==true);
            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x => x.CreateAt).Skip((page - 1) * 3).Limit(3);

            var comments = (await query.ToListAsync()).Adapt<List<CommentViewModel>>();
            return new EntityList<CommentViewModel>()
            {
                Total = (int)total,
                Entities = comments
            };
        }

        public async Task<List<RatingViewModel>> GetAmountRating(string id)
        {
            var result = await _comments.Aggregate()
                                        .Match(comment => comment.BookId == id && comment.IsCheck ==true)
                                       .Group(comment => comment.Rate, g => new
                                       {
                                           Key = g.Key,
                                           count = g.Count()
                                       }).SortByDescending(x => x.Key).Project(x => new RatingViewModel
                                       {
                                           Value = x.Key,
                                           Amount = x.count
                                       }).ToListAsync();

            var keys = result.Select(x => x.Value).ToList();
            var rateCase = new List<int>() { 1, 2, 3, 4, 5 };
            var exceptList = rateCase.Except(keys).ToList();
            foreach(var item in exceptList)
            {
                result.Add(new RatingViewModel()
                {
                    Value =item,
                    Amount = 0,
                });;
            }

            return result.OrderByDescending(x=>x.Value).ToList();
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            await _comments.InsertOneAsync(comment);
            return comment;
        }
        public async Task<bool> CheckedComment(string id)
        {
            var update = Builders<Comment>.Update.Set(x => x.IsCheck, true);

            await _comments.UpdateOneAsync(x=>x.Id==id,update);
            return true;
        }

        
        public string FormatDatetimeComment(DateTime createAt)
        {
            DateTime date = createAt.ToLocalTime();
            return date.Date.ToString() + " tháng" + date.Month.ToString() + " năm " + date.Year.ToString();
        }
    }
}
