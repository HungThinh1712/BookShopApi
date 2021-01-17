using BookShopApi.DatabaseSettings;
using BookShopApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Service
{

    public class NotificationService
    {
        private readonly IMongoCollection<Notification> _nofitications;

        public NotificationService(IBookShopDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _nofitications = database.GetCollection<Notification>(settings.NotificationsCollectionName);
        }

        public async Task<List<Notification>> GetAsync(string userId)
        {
            
            var notifications = await _nofitications.Find(notification => notification.UserId == userId).SortByDescending(notification=>notification.CreateAt).ToListAsync();

            var totalRead = await _nofitications.Find(notification => notification.UserId == userId && notification.Status==0).CountDocumentsAsync();
            foreach (var notification in notifications)
            {
                int time = ((int)DateTime.UtcNow.Subtract(notification.CreateAt).TotalMinutes);
                notification.TotalRead = (int)totalRead;
                if (time >= 1)
                {
                    notification.TimeAgo = time.ToString() + " phút" + " trước";
                }
                if (time >= 60 )
                {
                    notification.TimeAgo = ((int)DateTime.UtcNow.Subtract(notification.CreateAt).TotalHours).ToString() + " giờ" + " trước";
                }
                if (time >= 1440 )
                {
                    notification.TimeAgo = ((int)DateTime.UtcNow.Subtract(notification.CreateAt).TotalDays).ToString() + " ngày" + " trước";
                }
                
                if (time < 1)
                {
                    notification.TimeAgo = ((int)DateTime.UtcNow.Subtract(notification.CreateAt).TotalSeconds).ToString() + " giây" + " trước";
                }    
            }
            return notifications;
        }
           

        public async Task<Notification> GetbyIdAsync(string id) =>
           await _nofitications.Find<Notification>(notification => notification.Id == id).FirstOrDefaultAsync();
        public async Task<Notification> CreateAsync(Notification Notification)
        {
            await _nofitications.InsertOneAsync(Notification);
            return Notification;
        }

        public async Task UpdateAsync(string id, Notification notificationIn) =>
           await _nofitications.ReplaceOneAsync(notification => notification.Id == id, notificationIn);


        public async Task RemoveAsync(string id) =>
           await _nofitications.DeleteOneAsync(notification => notification.Id == id);
    }
}
