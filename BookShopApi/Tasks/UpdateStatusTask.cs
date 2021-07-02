using BookShopApi.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Tasks
{
   
    public class UpdateStatusTask : TaskBase
    {
       
        protected readonly ILogger<UpdateStatusTask> _logger;
        private readonly PromotionService _promotionService;
        private readonly OrderService _orderService;

        public UpdateStatusTask(ILogger<UpdateStatusTask> logger,PromotionService promotionService,OrderService orderService)
        {
            
            _logger = logger;
            _promotionService = promotionService;
            _orderService = orderService;
        }

        public override async Task<bool> ExecutionAsync()
        {
            await _promotionService.UpdateStatusAsync();
            await _orderService.UpdateStatusOrderAsync();
            return true;
        }
    }
}
