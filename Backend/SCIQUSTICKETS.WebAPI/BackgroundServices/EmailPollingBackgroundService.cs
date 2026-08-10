using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SCIQUSTICKETS.WebAPI.BackgroundServices
{
    public class EmailPollingBackgroundService : BackgroundService
    {
        private readonly ILogger<EmailPollingBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public EmailPollingBackgroundService(ILogger<EmailPollingBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Polling Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var emailChannelService = scope.ServiceProvider.GetRequiredService<IEmailChannelService>();
                        await emailChannelService.SyncMailboxesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Email Polling task.");
                }

                // Minimum 15 minutes as per requirements
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
