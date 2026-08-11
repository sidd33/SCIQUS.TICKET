// SCIQUSTICKETS.WebAPI/BackgroundServices/SlaBackgroundService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.BackgroundServices
{
	public class SlaBackgroundService : BackgroundService
	{
		private readonly ILogger<SlaBackgroundService> _logger;
		private readonly IServiceProvider _serviceProvider;

		public SlaBackgroundService(ILogger<SlaBackgroundService> logger, IServiceProvider serviceProvider)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("SLA Background Service is starting.");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _serviceProvider.CreateScope();
					var slaService = scope.ServiceProvider.GetRequiredService<ISlaService>();

					await slaService.ProcessAutoClosuresAsync();
					await slaService.ProcessBreachesAsync();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred executing SLA background pass.");
				}

				// Hourly, per module doc's recommended interval
				await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
			}
		}
	}
}