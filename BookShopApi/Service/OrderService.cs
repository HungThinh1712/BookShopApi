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
                                         Description = GetDescription(x.Items),
                                         Items = x.Items.Adapt<List<ItemInCartViewModel>>(),
                                         Status = x.Status,
                                         OrderAddress = x.OrderAddress,
                                         ConfirmStatus = x.ConfirmStatus,
                                         UserId = x.UserId,
                                         ShippingFee = x.ShippingFee,
                                         PaymentType = x.PaymentType,
                                         CancelReason = x.CancelReason
                                     }).ToListAsync()
             };
        } 

        public async Task<Order> CreateAsync(User user, int paymentType, List<ItemInCart> items,decimal shipingFee,decimal totalMoney,Promotion promotion)
        {
            var order = new Order();
            order.UserId = user.Id;
            order.OrderAddress = new OrderAddress()
            {
                FullName = user.FullName,
                Address = user.SpecificAddress,
                PhoneNumber = user.Phone
            };
            order.Items = items;
            order.ShippingFee = shipingFee;
            order.TotalMoney = totalMoney;
            order.PaymentType = paymentType;
            order.DiscountMoney = promotion !=null ? promotion.DiscountMoney : 0;
            order.PromotionCode = promotion != null ? promotion.PromotionCode : null;
            order.OrderId = DateTime.Now.ToString("yymmssff");
            order.Status = OrderStatus.DangChoXacNhan;
            if(promotion != null  && promotion.PromotionType == PromotionType.FreeShip){
                order.ShippingFee = 0;
            }
            //pay with momo
            if (paymentType == 2)
            {
                if (promotion.PromotionType == PromotionType.Discount)
                {
                    order.TotalMoney = totalMoney - promotion.DiscountMoney;
                }
                else
                {
                    order.TotalMoney = totalMoney - shipingFee;
                }
            }
            await _orders.InsertOneAsync(order);
            return order;
        }

        public async Task UpdateAsync(string id, Order orderIn) =>
           await _orders.ReplaceOneAsync(order => order.Id == id, orderIn);

        public async Task UpdateStatusOrderAsync()
        {
            var orders = await _orders.Find(x => x.Status == OrderStatus.DangGiaoHang).ToListAsync();
            foreach(var order in orders)
            {
                if ( (DateTime.UtcNow - order.UpdatedAt).TotalDays == 2  && order.ConfirmStatus ==ConfirmStatus.Seller)
                {
                    order.ConfirmStatus = ConfirmStatus.Both;
                    order.Status = OrderStatus.DaGiaoHang;
                    order.UpdatedAt = DateTime.UtcNow.AddHours(7);
                }
                await _orders.ReplaceOneAsync(x => x.Id == order.Id, order);
            }
        }

        public async Task  CancelOrder(string orderId, string reason)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id , orderId);
            var update = Builders<Order>.Update.Set(x => x.Status, OrderStatus.Huy)
                                                .Set(x => x.CancelReason , reason)
                                                .Set(x=>x.UpdatedAt,DateTime.UtcNow.AddHours(7));
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
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow.AddHours(7);
            if (status == OrderStatus.DaGiaoHang)
            {
                if (order.ConfirmStatus ==ConfirmStatus.None)
                {
                    if (IsAdmin)
                    {
                        order.Status = OrderStatus.DangGiaoHang;
                        order.ConfirmStatus = ConfirmStatus.Seller;
                    }    
                    else
                    {
                        order.ConfirmStatus = ConfirmStatus.Both;
                        order.Status = OrderStatus.DaGiaoHang;
                    }   
                }
                else
                {
                    order.ConfirmStatus = ConfirmStatus.Both;
                    order.Status = OrderStatus.DaGiaoHang;
                }
            }
            try
            {
                await _orders.ReplaceOneAsync(filter, order);
            }
            catch (Exception e)
            {
                throw e;
            }
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
                                         OrderAddress = x.OrderAddress,
                                         ConfirmStatus = x.ConfirmStatus,
                                         UserId = x.UserId,
                                         ShippingFee = x.ShippingFee,
                                         PaymentType = x.PaymentType,
                                         CancelReason =x.CancelReason
                                     }).ToListAsync()
            };
        }
        public async Task<List<decimal>> StatisticRevenue(int? year)
        {
            DateTime startDate =year!=null ? new DateTime((int)year, 1, 1) : DateTime.MinValue;
            DateTime endDate = year != null ? new DateTime((int)year + 1, 1, 1) : DateTime.MaxValue;
            var query = _orders.Aggregate().Match(x => (x.Status == OrderStatus.DaGiaoHang || x.Status ==OrderStatus.DaXacNhan || x.Status ==OrderStatus.DangGiaoHang)
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
        public async Task<List<TopFiveBooks>> GetTopFiveBooks(int? month, int? year)
        {
            DateTime startDate = year != null ? new DateTime((int)year, (int)month, 1) : DateTime.MinValue;
            DateTime endDate = year != null ? new DateTime((int)year, (int)month+1, 1) : DateTime.MaxValue;
            var query =  _orders.Find(x => (x.Status == OrderStatus.DaGiaoHang || x.Status == OrderStatus.DaXacNhan || x.Status == OrderStatus.DangGiaoHang)
            && (year == null || (x.CreateAt >= startDate && x.CreateAt < endDate))).Project(x => x.Items);
            var items = new List<ItemInCart>();
            foreach(var item in  await query.ToListAsync())
            {
                items.AddRange(item);
            }

            var results = from i in items
                          group i.Amount by i.Name into g
                          select new TopFiveBooks { BookName = g.Key, Sum = g.Sum() };
            return results.OrderByDescending(x=>x.Sum).Take(5).ToList();

        }


    }
}
