using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Books;
using BookShopApi.Models.ViewModels.Comments;
using Mapster;
using MongoDB.Driver;
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

        public async Task<List<Comment>> GetAsync() =>
            await _comments.Find(comment => true).ToListAsync();

        public async Task<List<CommentViewModel>> GetAsync(string id)
        {
            var result = await _comments.Aggregate()
                                        .Match(comment => comment.BookId == id && comment.Content != null && comment.Title != null)
                                       .Lookup<Comment, User, CommentWithUserFullName>(
                                            _users,
                                            x => x.UserId,
                                            x => x.Id,
                                            x => x.User).Project(x =>
                                           new CommentViewModel
                                           {
                                               Id = x.Id,
                                               Title = x.Title,
                                               Rate = x.Rate,
                                               Content = x.Content,
                                               CreateAt = x.CreateAt,
                                               UserFullName = x.User[0].FullName
                                           }).ToListAsync();
            var comments = result.Adapt<List<CommentViewModel>>();
            return comments;
        }

        public async Task<List<RatingViewModel>> GetAmountRating(string id)
        {
            var result = await _comments.Aggregate()
                                        .Match(comment => comment.BookId == id)
                                       .Group(comment => comment.Rate, g => new
                                       {
                                           Key = g.Key,
                                           count = g.Count()
                                       }).SortByDescending(x=>x.Key).Project(x => new RatingViewModel
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
    }
}
