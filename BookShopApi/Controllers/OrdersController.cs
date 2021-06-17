using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Orders;
using BookShopApi.Models.ViewModels.Users;
using BookShopApi.Service;
using Mapster;
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
        private readonly ProvinceService _provinceService;
        private readonly DistrictService _districtService;
        private readonly WardService _wardService;

        public OrdersController(OrderService orderService, UserService userService,
            ProvinceService provinceService,DistrictService districtService,WardService wardService)
        {
            _orderService = orderService;
            _userService = userService;
            _provinceService = provinceService;
            _districtService = districtService;
            _wardService = wardService;
        }
    

        [HttpGet]
        public async Task<ActionResult<EntityList<OrdersViewModel>>> Get([FromQuery] int page, [FromQuery] int pageSize, [FromQuery] OrderStatus status)
        {

            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var orders = await _orderService.GetAsync(userId,page,pageSize,status);
            
            
            return Ok(orders);
        }
        [HttpGet("Admin")]
        public async Task<ActionResult<EntityList<OrdersViewModel>>> GetAllOrder([FromQuery] int page, [FromQuery] int pageSize,OrderStatus? status)
        {

           
            var orders = await _orderService.GetAllAsync(page, pageSize,status);
            var users = await _userService.GetAsync();
            foreach(var order in orders.Entities)
            {
                var user = users.Where(x => x.Id == order.UserId).FirstOrDefault();
                order.UserName = user.FullName;
                order.PhoneNumber = user.Phone;
                order.UserAddress = user.SpecificAddress;
            }
            return Ok(orders);
        }
        [HttpGet("Admin/ConfirmOrder")]
        public async Task<ActionResult> ConfirmOrder(string orderId,OrderStatus status)
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var user = await _userService.GetAsync(userId);
            return Ok(await _orderService.ConfirmOrder(orderId,status,user.IsAdmin));
        }
        [HttpGet("CancelOrder")]
        public async Task<ActionResult> CancelOrder(string orderId, string reason)
        {
            await _orderService.CancelOrder(orderId, reason);
            return Ok();
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
