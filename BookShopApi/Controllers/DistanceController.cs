using BookShopApi.Functions;
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
        [HttpGet]
        public  ActionResult GetAll()        
        {
            DistanceMatrixRequest requestDistanceMatrix = new DistanceMatrixRequest();
            requestDistanceMatrix.AddOrigin(new Location("35 Lê Văn Chí, Linh Trung, Thủ Đức"));
            requestDistanceMatrix.Mode = TravelMode.driving;

            requestDistanceMatrix.AddDestination(new Location("Số 1, Võ Văn Ngân, Linh Trung, Thủ Đức"));
            var response = new DistanceMatrixService(new GoogleSigned("AIzaSyAts2fdjTVxp2RIdtf3K8kVd-LV2qJY22o"))
                   .GetResponse(requestDistanceMatrix);

            var message = response.Rows[0].Elements[0].distance.Value;
            return Ok(message);
        }
        [HttpPost]
        public ActionResult GetShippingFee(string distance)
        {
            
            return Ok(ShippingFee.CalculateShippingFee(distance));
        }
    }
}
