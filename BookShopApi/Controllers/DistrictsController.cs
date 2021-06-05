using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class DistrictsController
    {
        private readonly DistrictService _districtService;

        public DistrictsController(DistrictService districtService)
        {
            _districtService = districtService;
        }

        [HttpPost]
        public async Task<bool> Create([FromBody] JObject district)
        {
            var districts = district["LtsItem"];
            List<District> lstDistrict = districts
                   .Select(sc =>
                           new District()
                           {
                               Id = sc["abc"].ToString(),
                               Name = sc["name"].ToString().Trim(),
                               Alias = Unsign.convertToUnSign(sc["name"].ToString()).Trim(),
                               ProvinceId = sc["tinh_abc"].ToString()
                           }
                           ).ToList();
            await _districtService.CreateManyAsync(lstDistrict);
            return true;
        }
        [HttpGet]
        public async Task<List<District>> Get(string id)
        {

            return await _districtService.GetByProvinceIdAsync(id);
        }
    }
}
