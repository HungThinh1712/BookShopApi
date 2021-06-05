using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Orders;
using BookShopApi.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly UserService _userService;

        public OrdersController(OrderService orderService, UserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }
    

        [HttpGet]
        public async Task<ActionResult<EntityList<OrdersViewModel>>> Get([FromQuery] int page, [FromQuery] int pageSize)
        {

            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var orders = await _orderService.GetAsync(userId,page,pageSize);
            foreach(var order in orders.Entities)
            {
                //Payment with momo
                if (order.PaymentType == 2)
                    order.TotalMoney = 0.ToString();
            }
            
            return Ok(orders);
        }
        [HttpGet("Admin")]
        public async Task<ActionResult<EntityList<OrdersViewModel>>> GetAllOrder([FromQuery] int page, [FromQuery] int pageSize,int status)
        {

           
            var orders = await _orderService.GetAllAsync(page, pageSize,status);
            var users = await _userService.GetAsync();
            foreach(var order in orders.Entities)
            {
                var user = users.Where(x => x.Id == order.UserId).FirstOrDefault();
                order.UserName = user.FullName;
                order.PhoneNumber = user.Phone;
            }
            return Ok(orders);
        }
        [HttpGet("Admin/ConfirmOrder")]
        public async Task<ActionResult> ConfirmOrder(string orderId)
        {
            return Ok(await _orderService.ConfirmOrder(orderId));
        }
        [HttpGet("StatisticByMonth")]
        public async Task<ActionResult> GetMonthsStatistic([FromQuery] int? year)
        {
            return Ok(await _orderService.StatisticRevenue(year));
        }
        [HttpGet("[action]")]
        public async Task<ActionResult> GetOrder([FromQuery] string id)
        {
            return Ok(await _orderService.GetOrderAsync(id));
        }
    }
}
