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
    public class WardsController
    {
        private readonly WardService _wardService;

        public WardsController(WardService wardService)
        {
            _wardService = wardService;
        }

        [HttpPost]
        public async Task<bool> Create([FromBody] JObject ward)
        {
            var wards = ward["LtsItem"];
            List<Ward> lstWard = wards
                   .Select(sc =>
                           new Ward()
                           {
                               Id = sc["abc"].ToString(),
                               Name = sc["name"].ToString().Trim(),
                               Alias = Unsign.convertToUnSign(sc["name"].ToString()).Trim(),
                               DistrictId = sc["huyen_abc"].ToString()
                           }
                           ).ToList();
            await _wardService.CreateManyAsync(lstWard);
            return true;
        }
        [HttpGet]
        public async Task<List<Ward>> Get(string id)
        {

            return await _wardService.GetByDistrictIdAsync(id);
        }
    }
}
