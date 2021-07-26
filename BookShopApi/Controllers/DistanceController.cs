using BookShopApi.Functions;
using BookShopApi.Service;
using Google.Maps;
using Google.Maps.DistanceMatrix;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistanceController: ControllerBase
    {
        private readonly UserService _userService;
        public DistanceController(UserService userService)
        {
            _userService = userService;
        }
        //[HttpGet]
        //public async Task<ActionResult> GetDistanceAndShippingFee()        
        //{
            

        //    return Ok(new { distance= distance, shippingFee = shippingFee });

            
        //}
        
    }
}
