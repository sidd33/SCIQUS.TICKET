using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.BackgroundServices
{
    public class SupportPlanExpiryJob : BackgroundService
    {
        private readonly ILogger<SupportPlanExpiryJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SupportPlanExpiryJob(ILogger<SupportPlanExpiryJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SupportPlanExpiryJob running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var supportPlanService = scope.ServiceProvider.GetRequiredService<ISupportPlanService>();
                        await supportPlanService.ValidateAndExpirePlansAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing SupportPlanExpiryJob.");
                }

                // Run every 1 hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
