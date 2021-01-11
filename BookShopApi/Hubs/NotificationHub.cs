using BookShopApi.Hubs.Clients;
using BookShopApi.Models;
using BookShopApi.Service;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Hubs
{

    public class NotificationHub : Hub<INotificationClient>
    {
       
    }
}
