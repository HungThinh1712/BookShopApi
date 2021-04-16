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

        public CommentService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _comments = database.GetCollection<Comment>(settings.CommentsCollectionName);
            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public async Task<Comment> GetByUserIdAsync(string id) =>
           await _comments.Find<Comment>(comment => comment.UserId == id).FirstOrDefaultAsync();

        public async Task<List<Comment>> GetByIdAsync(string id) =>
            await _comments.Find(comment => comment.BookId ==id).ToListAsync();
        //public async Task<List<Comment>> GetByUserIdAsync(string id) =>
        //    await _comments.Find(comment => comment.UserId == id).ToListAsync();


        public async Task<EntityList<CommentViewModel>> GetAsync(string id, int page = 1)
        {
            if (page == 0)
                page = 1;

            var query = _comments.Find(x => x.BookId == id);
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
                                        .Match(comment => comment.BookId == id)
                                       .Group(comment => comment.Rate, g => new
                                       {
                                           Key = g.Key,
                                           count = g.Count()
                                       }).SortByDescending(x => x.Key).Project(x => new RatingViewModel
                                       {
                                           Value = x.Key,
                                           Amount = x.count
                                       }).ToListAsync();

            return result;
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            await _comments.InsertOneAsync(comment);
            return comment;
        }

        public async Task UpdateAsync(string id, Comment commentIn) =>
           await _comments.ReplaceOneAsync(comment => comment.Id == id, commentIn);


        public async Task RemoveAsync(string id) =>
           await _comments.DeleteOneAsync(comment => comment.Id == id);
        public string FormatDatetimeComment(DateTime createAt)
        {
            DateTime date = createAt.ToLocalTime();
            return date.Date.ToString() + " tháng" + date.Month.ToString() + " năm " + date.Year.ToString();
        }
    }
}
