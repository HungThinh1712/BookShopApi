using BookShopApi.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookShopApi
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private Timer _timer;
        private readonly PromotionService _promotionService;

        public TimedHostedService(ILogger<TimedHostedService> logger,PromotionService promotionService)
        {
            _logger = logger;
            _promotionService = promotionService;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            await DoWork();
        }

        private async Task DoWork()
        {
            await _promotionService.UpdateStatusAsync();
            _logger.LogInformation(
           "Consume Scoped Service Hosted Service is working.");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
