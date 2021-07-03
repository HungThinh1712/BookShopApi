using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels.Promotion;
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
    public class PromotionController : ControllerBase
    {
        private readonly PromotionService _promotion;
        public PromotionController(PromotionService promotion)
        {
            _promotion = promotion;
        }
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Promotion promotion)
        {
            
            return Ok(await _promotion.CreateAsync(promotion));
        }
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] Promotion promotion)
        {
            await _promotion.UpdateAsync(promotion);
            return Ok(true);
        }
        [HttpGet("Cancel")]
        public async Task<ActionResult> Cancel([FromQuery] string id)
        {
            await _promotion.CancelPromotionAsync(id);
            return Ok(true);
        }
        [HttpGet("Detail")]
        public async Task<ActionResult> GetDetail([FromQuery] string id)
        {
            var promotion = await _promotion.GetPromotionByIdAsync(id);
            return Ok(promotion);
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] int page, [FromQuery] int pageSize,[FromQuery] PromotionStatus status)
        {
            var promotions = await _promotion.GetAsync(page, pageSize, status);
            
            return Ok(promotions);
        }
        [HttpPost("Apply")]
        public async Task<ActionResult> ApplyPromotion([FromBody] ApplyPromotionModel promotion)
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());

           var result = await _promotion.ApplyPromotion(promotion.PromotionCode, userId,promotion.TotalMoney,promotion.BookIds);
            if (result != null)
                return Ok(result);
            return BadRequest();
        }
        [HttpGet("GetPromotionsByMe")]
        public async Task<ActionResult> GetPromotionsByMe([FromQuery] decimal totalMoney, [FromQuery]List<string> bookIds)
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());

            return Ok(await _promotion.GetPromotionByMe(userId,totalMoney,bookIds));
        }
    }
}
