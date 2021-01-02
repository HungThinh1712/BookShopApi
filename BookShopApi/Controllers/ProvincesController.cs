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
    public class ProvincesController
    {
        private readonly ProvinceService _provinceService;

        public ProvincesController(ProvinceService provinceService)
        {
            _provinceService = provinceService;
        }

        [HttpPost]
        public async Task<bool> Create([FromBody] JObject province)
        {
            var provinces = province["LtsItem"];
            List<Province> lstprovince = provinces
                   .Select(sc =>
                           new Province()
                           {
                               Id = sc["abc"].ToString(),
                               Name = sc["name"].ToString().Trim(),
                               Alias = Unsign.convertToUnSign(sc["name"].ToString()).Trim()
                           }
                           ).ToList();
            foreach (var item in lstprovince)
                await _provinceService.CreateAsync(item);
            return true;
        }
        [HttpGet]
        public async Task<List<Province>> Get()
        {

            return await _provinceService.GetAllAsync();
        }

    }
}
