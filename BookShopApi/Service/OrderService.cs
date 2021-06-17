using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels;
using BookShopApi.Models.ViewModels.Orders;
using Mapster;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<EntityList<OrdersViewModel>> GetAsync(string userId, int page, int pageSize, OrderStatus status)
        {
            var query = _orders.Find(order => order.UserId == userId && order.Status==status);

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
                                         TotalMoney = x.TotalMoney,
                                         ShippingFee = x.ShippingFee,
                                         Description = GetDescription(x.Items),
                                         Items = x.Items.Adapt<List<ItemInCartViewModel>>(),
                                         Status = x.Status,
                                         PaymentType = x.PaymentType
                                     }).ToListAsync()
             };
        } 

        public async Task<Order> CreateAsync(string userId, int paymentType, List<ItemInCart> items,decimal shipingFee,decimal totalMoney)
        {
            var order = new Order();
            order.UserId = userId;
            order.Items = items;
            order.ShippingFee = shipingFee;
            order.TotalMoney = totalMoney;
            order.PaymentType = paymentType;
            order.OrderId = DateTime.Now.ToString("yymmssff");
            order.Status = OrderStatus.DangChoXacNhan;
            await _orders.InsertOneAsync(order);
            return order;
        }

        public async Task UpdateAsync(string id, Order orderIn) =>
           await _orders.ReplaceOneAsync(order => order.Id == id, orderIn);

        public async Task  CancelOrder(string orderId, string reason)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id , orderId);
            var update = Builders<Order>.Update.Set(x => x.Status, OrderStatus.Huy)
                                                .Set(x => x.CancelReason , reason);
            await _orders.UpdateOneAsync(filter, update);
        }

        public async Task<Order> GetOrderAsync(string id) =>
          await _orders.Find(order => order.Id == id).FirstOrDefaultAsync();
        public async Task<List<Order>> GetOrdersAsync(string id) =>
          await _orders.Find(order => order.UserId == id).ToListAsync();


        public async Task RemoveAsync(string id) =>
           await _orders.DeleteOneAsync(order => order.Id == id);

        private string GetTotalMoney(List<ItemInCart> itemInCarts)
        {
            decimal total = 0;
            foreach (var item in itemInCarts)
                total += item.Price *item.Amount;
            return total.ToString();
        }
        public async Task<bool> ConfirmOrder(string id,OrderStatus  status,bool IsAdmin)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var order = await _orders.Find(filter).FirstOrDefaultAsync();
            var update = Builders<Order>.Update.Set(x => x.Status, status);
            if (status == OrderStatus.DaGiaoHang)
            {
                if (order.ConfirmStatus ==ConfirmStatus.None)
                {
                    if (IsAdmin)
                        update = Builders<Order>.Update.Set(x => x.ConfirmStatus, ConfirmStatus.Seller);
                    else
                    {
                        update = Builders<Order>.Update.Set(x => x.Status, status).
                        Set(x => x.ConfirmStatus, ConfirmStatus.Both);
                    }   
                }
                else
                {
                     update = Builders<Order>.Update.Set(x => x.Status, status).
                        Set(x=>x.ConfirmStatus,ConfirmStatus.Both);
                }
            }
            await _orders.UpdateOneAsync(filter,update);
            return true;
        }

        public async Task UpdateStatusRateAsync(Order order)
        {
            var update = Builders<Order>.Update.Set(x => x.Items, order.Items);
            await _orders.UpdateOneAsync(x => x.Id == order.Id, update);
        }

        private string GetDescription(List<ItemInCart> items)
        {
            string count = items.Count >= 2 ? (items.Count - 1).ToString() : string.Empty;
            if (items.Count > 1)
                return items[0].Name + " và " + count.ToString() + " cuốn sách khác...";
            else
                return items[0].Name;


        }
        public async Task<EntityList<OrdersViewModel>> GetAllAsync(int page, int pageSize,OrderStatus? status)
        {
            var query = _orders.Find(order => true);


            if (status != null)
            {
                query = _orders.Find(order => order.Status == status);
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
                                         TotalMoney = x.TotalMoney,
                                         Description = GetDescription(x.Items),
                                         Items = x.Items.Adapt<List<ItemInCartViewModel>>(),
                                         Status = x.Status,
                                         ConfirmStatus = x.ConfirmStatus,
                                         UserId = x.UserId,
                                         ShippingFee = x.ShippingFee,
                                         PaymentType = x.PaymentType
                                     }).ToListAsync()
            };
        }
        public async Task<List<decimal>> StatisticRevenue(int? year)
        {
            DateTime startDate =year!=null ? new DateTime((int)year, 1, 1) : DateTime.MinValue;
            DateTime endDate = year != null ? new DateTime((int)year + 1, 1, 1) : DateTime.MaxValue;
            var query = _orders.Aggregate().Match(x => x.Status == OrderStatus.DaGiaoHang
            && (year==null || (x.CreateAt >=startDate && x.CreateAt < endDate))).
                Group(x => x.CreateAt.Month, x => new
                {
                    month =x.Key,
                    TotalPrice = x.Sum(x => x.Items.Sum(y => y.TotalMoney))
                });
            var orders =  await query.ToListAsync();
            List<int> months = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var orderMonths = orders.Select(x => x.month).ToList() ;
            var differentMonh = months.Except(orderMonths).ToList();
            foreach(var item in differentMonh)
            {
                orders.Add(new { month = item, TotalPrice = (decimal)0 });
            }
            return orders.OrderBy(x => x.month).Select(x=>x.TotalPrice).ToList();
                
        } 

      
    }
}
