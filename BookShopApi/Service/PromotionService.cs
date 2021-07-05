using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Promotion;
using Mapster;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Service
{

    public class PromotionService
    {
        private readonly IMongoCollection<Promotion> _promotions;

        public PromotionService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _promotions = database.GetCollection<Promotion>(settings.PromotionsCollectionName);
        }

        public async Task<bool> CheckPromotionAsync(string promotionCode)
        {
            var promotion = await _promotions.Find(x => x.PromotionCode == promotionCode).FirstOrDefaultAsync();
            return promotion != null;
        }

        public async Task<PromotionViewModel> ApplyPromotion(string promotionCode,string userId,decimal totalMoney, List<string> bookIds)
        {
            var promotion = await _promotions.Find(x => x.PromotionCode == promotionCode).FirstOrDefaultAsync();
            if (promotion == null)
                return null;
            else
            {
                var date = DateTime.UtcNow.Date;
                promotion.StartDate = promotion.StartDate.Date;
                promotion.EndDate = new DateTime(promotion.EndDate.Year, promotion.EndDate.Month, promotion.EndDate.Day, 23, 59, 0);
                if(promotion.StartDate <= DateTime.UtcNow.Date 
                  && promotion.EndDate > DateTime.UtcNow.Date
                  && bookIds.All(x=>promotion.BookIds.Contains(x))
                  && promotion.CustomerIds.Contains(userId) 
                  && promotion.MinMoney <= totalMoney
                  && promotion.CountApply > 0
                  && !promotion.CustomerApplied.Contains(userId))
                {
                    return promotion.Adapt<PromotionViewModel>();
                }
                else 
                { 
                    return null; 
                }
            }
        }

        public async Task<EntityList<Promotion>> GetAsync(int page, int pageSize,PromotionStatus status)
        {
            var query = _promotions.Find(x => x.Status == status).Skip((page -1)* pageSize).SortByDescending(x=>x.CreatedAt).Limit(pageSize);
            var total = await query.CountDocumentsAsync();
            return new EntityList<Promotion>()
            {
                Total = (int)total,
                Entities = await query.ToListAsync()
            };
        }

        public async Task<Promotion> GetAsync(string promotionCode) =>
           await _promotions.Find<Promotion>(promotion => promotion.PromotionCode == promotionCode).FirstOrDefaultAsync();
        public async Task<Promotion> GetPromotionByIdAsync(string id) =>
          await _promotions.Find<Promotion>(promotion => promotion.Id == id).FirstOrDefaultAsync();

        public async Task<bool> UpdatePromotionAfterApply(string userId,string promotionCode)
        {
            var promotion = await _promotions.Find(x => x.PromotionCode == promotionCode).FirstOrDefaultAsync();
            promotion.CountApply = promotion.CountApply - 1;
            promotion.CustomerApplied.Add(userId);
            await _promotions.ReplaceOneAsync(x => x.Id == promotion.Id, promotion);
            return true;
        }



        public async Task<bool> CreateAsync(Promotion promotion)
        {
            var promotionTest = await GetAsync(promotion.PromotionCode);
            if(promotionTest != null)
            {
                return false;
            }
            promotion.Status = PromotionStatus.InActive;
            promotion.StartDate = promotion.StartDate.Date;
            promotion.EndDate = promotion.EndDate.Date;
            if (promotion.PromotionType == PromotionType.FreeShip) promotion.DiscountMoney = 0;
            await _promotions.InsertOneAsync(promotion);
            return true;
        }

        public async Task<bool> UpdateAsync(Promotion promotion)
        {
            var promotionInDb =await _promotions.Find(x => x.Id == promotion.Id).FirstOrDefaultAsync();
            promotion = promotion.Adapt(promotionInDb);
            promotion.StartDate = promotion.StartDate.AddHours(7);
            promotion.EndDate = promotion.EndDate.AddHours(7);
            try
            {
                await _promotions.ReplaceOneAsync(x => x.Id == promotion.Id, promotion);
            }
            catch(Exception e)
            {
               throw e;
            }
            return true;
        }
        public async Task<bool> CancelPromotionAsync(string id)
        {
            var update = Builders<Promotion>.Update.Set(x => x.Status , PromotionStatus.InActive);
            await _promotions.UpdateOneAsync(x => x.Id == id, update);
            return true;
        }

        public async Task<bool> UpdateStatusAsync()
        {
            var now = DateTime.UtcNow.AddHours(7).Date;
            var promotions = await _promotions.Find(x => x.Status != PromotionStatus.Expired ).ToListAsync();
            foreach(var promotion in promotions)
            {
                if(promotion.StartDate <= now && promotion.EndDate > now && promotion.Status ==PromotionStatus.Active)
                {
                    promotion.Status = PromotionStatus.OnGoing;
                }
                if(promotion.EndDate <  now)
                {
                    promotion.Status = PromotionStatus.Expired;
                }
                await _promotions.ReplaceOneAsync(x => x.Id == promotion.Id, promotion);
            }
            return true;
           
        }

        public async Task<List<Promotion>> GetPromotionByMe(string userId, decimal totalMoney, List<string> bookIds)
        {
            var promotions = await _promotions.Find(promotion => promotion.Status == PromotionStatus.OnGoing
                  && bookIds.All(x => promotion.BookIds.Contains(x))
                  && promotion.CustomerIds.Contains(userId)
                  && promotion.MinMoney <= totalMoney
                  && promotion.CountApply > 0
                  && !promotion.CustomerApplied.Contains(userId)).ToListAsync();
            return promotions;
            
        }


    }
}
