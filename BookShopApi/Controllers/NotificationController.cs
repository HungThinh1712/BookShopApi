﻿using BookShopApi.Functions;
using BookShopApi.Hubs;
using BookShopApi.Hubs.Clients;
using BookShopApi.Models;
using BookShopApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly NotificationService _notificationService;
        private readonly UserService _userService;

        public NotificationController(IHubContext<NotificationHub, INotificationClient> notificationHub,
                                        NotificationService notificationService, UserService userService)
        {
            _notificationHub = notificationHub;
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpPost("messages")]
        public async Task Post(Notification notification)
        {
            // run some logic...
            notification.CreateAt = DateTime.UtcNow;
            notification.Status = 0;
            if (notification.Type == "Confirm")
            {
                notification.ImgUrl = "https://img.icons8.com/bubbles/2x/admin-settings-male.png";
            }
            else if (notification.Type == "Delivery")
            {
                notification.ImgUrl = "https://www.pngitem.com/pimgs/m/485-4853792_white-motorbike-icon-delivery-png-transparent-png.png";
            }
            else if (notification.Type == "ConfirmDelivery")
            {
                notification.ImgUrl = "https://c8.alamy.com/comp/2AP68RG/package-delivery-color-icon-courier-service-parcel-delivering-deliveryman-with-box-and-invoice-postman-holding-cardboard-package-postal-service-2AP68RG.jpg";
            }
            else if (notification.Type == "Cancel")
            {
                notification.ImgUrl = "https://png.pngtree.com/png-clipart/20190614/original/pngtree-cancel-icon-wood-png-image_3604377.jpg";
            }
            await _notificationService.CreateAsync(notification);

            await _notificationHub.Clients.All.ReceiveMessage(notification);
        }
        [HttpGet("ChangeStatus")]
        public async Task ChangeStatus(string id)
        {
            // run some logic...
            var notification = await _notificationService.GetbyIdAsync(id);
            notification.Status = 1;
            await _notificationService.UpdateAsync(id, notification);
            //await _notificationHub.Clients.All.ReceiveMessage(notification);
        }
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            // run some logic...
            List<Notification> notifications = new List<Notification>();
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            if (!string.IsNullOrEmpty(userId))
            {
                notifications = await _notificationService.GetAsync(userId);
            }
            
            return Ok(notifications);

        }

        [HttpGet("DeleteNoti")]
        public async Task DeleteNoti(string id)
        {
            // run some logic...

            var returedNotification = await _notificationService.GetbyIdAsync(id);

            await _notificationService.RemoveAsync(id);


            await _notificationHub.Clients.All.ReceiveMessage(returedNotification);
        }

        [HttpPost("SendNotiToAmin")]
        public async Task SendNotiToAmin(Notification notification)
        {
            // run some logic...    
            try
            {
                var userAdmin = await _userService.GetAdminAsync();

                notification.UserId = userAdmin.Id;
                notification.CreateAt = DateTime.UtcNow;
                notification.Status = 0;
                notification.ImgUrl = (await _userService.GetAsync(notification.SenderId)).ImgUrl;
               

                await _notificationService.CreateAsync(notification);

                await _notificationHub.Clients.All.ReceiveMessage(notification);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
