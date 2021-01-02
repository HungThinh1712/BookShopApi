using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class ShoppingCartService
    {
        private readonly IMongoCollection<ShoppingCart> _shoppingCarts;

        public ShoppingCartService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _shoppingCarts = database.GetCollection<ShoppingCart>(settings.ShoppingCartsCollectionName);
        }

        public async Task<List<ShoppingCart>> GetAsync() =>
            await _shoppingCarts.Find(shoppingCart => true).ToListAsync();
        public async Task<List<ShoppingCart>> GetByBookIdAsync() =>
           await _shoppingCarts.Find(shoppingCart => true).ToListAsync();

        public async Task<ShoppingCart> GetAsync(string id) =>
           await _shoppingCarts.Find<ShoppingCart>(shoppingCart => shoppingCart.Id == id).FirstOrDefaultAsync();
        public async Task<ShoppingCart> GetByUserIdAsync(string userId) =>
         await _shoppingCarts.Find<ShoppingCart>(shoppingCart => shoppingCart.UserId == userId).FirstOrDefaultAsync();

        public async Task<ShoppingCart> CreateAsync(ShoppingCart shoppingCart)
        {
            await _shoppingCarts.InsertOneAsync(shoppingCart);
            return shoppingCart;
        }

        public async Task UpdateAsync(string id, ShoppingCart shoppingCartIn) =>
           await _shoppingCarts.ReplaceOneAsync(shoppingCart => shoppingCart.UserId == id, shoppingCartIn);


        public async Task RemoveAsync(string id) =>
           await _shoppingCarts.DeleteOneAsync(shoppingCart => shoppingCart.Id == id);
        public async Task RemoveCartItemAsync(string userId, string bookId)
        {
            var shoppingCart = await _shoppingCarts.Find<ShoppingCart>(shoppingCart => shoppingCart.UserId == userId).FirstOrDefaultAsync();
            shoppingCart.ItemInCart.Remove(GetItemInCartByBookId(bookId,shoppingCart.ItemInCart));
            await _shoppingCarts.ReplaceOneAsync(shoppingCart => shoppingCart.UserId == userId, shoppingCart);
        }

        public async Task<List<ItemInCart>> UpdateAmountAsync(string userId,string bookId,int amount) {
            var cart = await _shoppingCarts.Find<ShoppingCart>(shoppingCart => shoppingCart.UserId == userId).FirstOrDefaultAsync();
            var itemInCart = GetItemInCartByBookId(bookId, cart.ItemInCart);
            itemInCart.Amount =amount;
            await _shoppingCarts.ReplaceOneAsync(shoppingCart => shoppingCart.UserId == cart.UserId, cart);
            return cart.ItemInCart;
        }
          
        private ItemInCart GetItemInCartByBookId(string bookId,List<ItemInCart> itemInCarts)
        {
            foreach(var item in itemInCarts)
            {
                if (item.BookId == bookId)
                    return item;
            }
            return null;
        }
        
    }
}
