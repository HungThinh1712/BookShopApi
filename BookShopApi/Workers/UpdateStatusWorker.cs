using BookShopApi.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Workers
{
   
    public class UpdateStatusWorker : WorkerBase
    {
        public UpdateStatusWorker(IServiceProvider services,
            ILogger<UpdateStatusWorker> logger
            , IConfiguration configuration) : base(services, logger, configuration)
        {

        }

        protected override async Task ExecuteTasks(IServiceProvider services)
        {
            await services.GetRequiredService<UpdateStatusTask>().ExecutionAsync();
        }
    }
}
