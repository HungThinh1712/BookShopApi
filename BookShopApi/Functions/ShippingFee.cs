using Google.Maps;
using Google.Maps.DistanceMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookShopApi.Functions
{
    public static class ShippingFee
    {
        public static decimal CalculateShippingFee(string distance)
        {
            const decimal basicFee = 15000;
            const decimal unit = 500;
            var distanceString = Regex.Match(distance, @"(\d+(\.\d+)?)|(\.\d+)").Value;
            var distanceDecimal = decimal.Parse(distanceString);     
            if(distanceDecimal > 20 )
            {
                return (distanceDecimal - 20) * unit + basicFee ;
            }
             
            return basicFee;
        }
        public static string GetDistance(string address)
        {
            DistanceMatrixRequest requestDistanceMatrix = new DistanceMatrixRequest();
            requestDistanceMatrix.AddOrigin(new Location("Số 1, Võ Văn Ngân, Linh Trung, Thủ Đức"));
            requestDistanceMatrix.Mode = TravelMode.driving;

            requestDistanceMatrix.AddDestination(new Location(address));
            var response = new DistanceMatrixService(new GoogleSigned("AIzaSyAts2fdjTVxp2RIdtf3K8kVd-LV2qJY22o"))
                   .GetResponse(requestDistanceMatrix);

            return response.Rows[0].Elements[0].distance.Text;
        }
    }
}
