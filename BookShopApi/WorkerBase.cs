using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookShopApi
{
    public abstract class WorkerBase : BackgroundService
    {
        protected readonly ILogger _logger;
        protected readonly IServiceProvider _services;
        protected readonly IConfiguration _configuration;

        public WorkerBase(IServiceProvider services, ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _configuration = configuration;
        }

        protected abstract Task ExecuteTasks(IServiceProvider services);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var serviceScope = _services.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;

                    try
                    {
                        await ExecuteTasks(services);
                    }
                    catch (Exception ex)
                    {
                        services.GetRequiredService<ILogger<Program>>().LogError(ex, "An error occurred.");
                    }
                }

                _logger.LogInformation("WorkerBase running at: {time}", DateTimeOffset.Now);


                //One minute
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
