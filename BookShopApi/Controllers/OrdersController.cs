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
        public async Task<ActionResult<EntityList<OrdersViewModel>>> Get([FromQuery] int page, [FromQuery] int pageSize)
        {

            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var orders = await _orderService.GetAsync(userId,page,pageSize);
            
            
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
                order.UserAddress = await GetAddress(user);
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
        private async Task<string> GetAddress(User user)
        {
            var returnedUser = user.Adapt<UserViewModel>();
            var provinces = await _provinceService.GetAllAsync();
            var wards = await _wardService.GetByDistrictIdAsync(returnedUser.DistrictId);
            var districts = await _districtService.GetByProvinceIdAsync(returnedUser.ProvinceId);
            if (returnedUser.ProvinceId != null)
            {
                returnedUser.ProvinceName = provinces.Where(x => x.Id == returnedUser.ProvinceId).FirstOrDefault().Name;
                returnedUser.DistrictName = districts.Where(x => x.Id == returnedUser.DistrictId).FirstOrDefault().Name;
                returnedUser.WardName = wards.Where(x => x.Id == returnedUser.WardId).FirstOrDefault().Name;
                return returnedUser.SpecificAddress + ", " + returnedUser.WardName + ", " + returnedUser.DistrictName + ", " + returnedUser.ProvinceName;
            }
            else
            {
                return "Chưa cập nhật địa chỉ giao hàng";
            }
        }
    }
}
