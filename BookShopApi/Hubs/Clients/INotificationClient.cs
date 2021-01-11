using BookShopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Hubs.Clients
{
    public interface INotificationClient
    {
        Task ReceiveMessage(Notification notification);
    }
}
