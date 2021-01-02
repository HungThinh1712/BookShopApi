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

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }
    

        [HttpGet]
        public async Task<ActionResult<EntityList<OrdersViewModel>>> Get([FromQuery] int page, [FromQuery] int pageSize)
        {

            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var orders = await _orderService.GetAsync(userId,page,pageSize);
            
            return Ok(orders);
        }


    }
}
