using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels;
using BookShopApi.Models.ViewModels.Orders;
using Mapster;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Service
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orders;


        public OrderService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _orders = database.GetCollection<Order>(settings.OrdersCollectionName);
        }

        public async Task<EntityList<OrdersViewModel>> GetAsync(string userId, int page = 1, int pageSize = 4)
        {
            var query = _orders.Find(order => order.UserId == userId);

            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x=>x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<OrdersViewModel>()
            {
                Total = (int)total,
                Entities = await query.Project(x =>
                                     new OrdersViewModel
                                     {
                                         Id = x.Id,
                                         OrderId = x.OrderId,
                                         CreateAt = Convert.ToDateTime(x.CreateAt).ToString("dd-MM-yyyy"),
                                         TotalMoney = GetTotalMoney(x.Items),
                                         Description = GetDescription(x.Items),
                                         Items = x.Items.Adapt<List<ItemInCartViewModel>>(),
                                         Status = x.Status,
                                         PaymentType = x.PaymentType
                                     }).ToListAsync()
             };
        } 

        public async Task<Order> CreateAsync(string userId, int paymentType, List<ItemInCart> items)
        {
            var order = new Order();
            order.UserId = userId;
            order.Items = items;
            order.PaymentType = paymentType;
            order.OrderId = DateTime.Now.ToString("yymmssff");
            order.Status = "Đang chờ xác nhận";
            await _orders.InsertOneAsync(order);
            return order;
        }

        public async Task UpdateAsync(string id, Order orderIn) =>
           await _orders.ReplaceOneAsync(order => order.Id == id, orderIn);


        public async Task RemoveAsync(string id) =>
           await _orders.DeleteOneAsync(order => order.Id == id);

        private string GetTotalMoney(List<ItemInCart> itemInCarts)
        {
            decimal total = 0;
            foreach (var item in itemInCarts)
                total += item.Price *item.Amount;
            return total.ToString("#.000");
        }
        public async Task<bool> ConfirmOrder(string id)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var update = Builders<Order>.Update.Set(x => x.Status, "Đã xác nhận");
            await _orders.UpdateOneAsync(filter,update);
            return true;
        }
        private string GetDescription(List<ItemInCart> items)
        {
            string count = items.Count >= 2 ? (items.Count - 1).ToString() : string.Empty;
            if (items.Count > 1)
                return items[0].Name + " và " + count.ToString() + " cuốn sách khác...";
            else
                return items[0].Name;


        }
        public async Task<EntityList<OrdersViewModel>> GetAllAsync(int page = 1, int pageSize = 10,int status=0)
        {
            var query = _orders.Find(order => true);
            
            if (status == 1)
            {
                query = _orders.Find(order => order.Status =="Đã xác nhận");
            }
            else if(status==2)
            {
                query = _orders.Find(order => order.Status == "Đang chờ xác nhận");
            }    

            var total = await query.CountDocumentsAsync();
            query = query.SortByDescending(x=>x.CreateAt).Skip((page - 1) * pageSize).Limit(pageSize);
            return new EntityList<OrdersViewModel>()
            {
                Total = (int)total,
                Entities = await query.Project(x =>
                                     new OrdersViewModel
                                     {
                                         Id = x.Id,
                                         OrderId = x.OrderId,
                                         CreateAt = Convert.ToDateTime(x.CreateAt).ToString("dd-MM-yyyy"),
                                         TotalMoney = GetTotalMoney(x.Items),
                                         Description = GetDescription(x.Items),
                                         Items = x.Items.Adapt<List<ItemInCartViewModel>>(),
                                         Status = x.Status,
                                         UserId = x.UserId,
                                         PaymentType = x.PaymentType
                                     }).ToListAsync()
            };
        }

      
    }
}
