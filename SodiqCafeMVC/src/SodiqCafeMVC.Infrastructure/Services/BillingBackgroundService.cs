using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SodiqCafeMVC.Application.Interfaces;

namespace SodiqCafeMVC.Infrastructure.Services
{
    public class BillingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BillingBackgroundService> _logger;

        public BillingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BillingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Billing Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
                    var telegramService = scope.ServiceProvider.GetRequiredService<ISystemTelegramBotService>();

                    _logger.LogInformation("Billing Cron: Checking for overdue cafes.");
                    await billingService.CheckAndSuspendOverdueCafesAsync();

                    // If today is the 1st day of the month, generate invoices for all cafes for the PREVIOUS month
                    // This logic should ideally be careful not to generate multiple times per day. 
                    // We check if an invoice for the previous month already exists in BillingService.
                    if (now.Day == 1)
                    {
                        var lastMonth = now.AddMonths(-1);
                        _logger.LogInformation($"Billing Cron: Generating monthly invoices for {lastMonth:yyyy-MM}.");
                        await billingService.GenerateAllMonthlyInvoicesAsync(lastMonth);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing billing background task.");
                }

                // Wait for the next day. Since we want to run this roughly at midnight, 
                // a simple approach is to calculate time until next midnight.
                var tomorrow = now.AddDays(1).Date; // Midnight tomorrow
                var delay = tomorrow - DateTime.UtcNow;
                
                // If the calculation took us past midnight, delay might be negative. Wait at least 1 minute.
                if (delay.TotalMilliseconds <= 0) delay = TimeSpan.FromMinutes(1);

                _logger.LogInformation($"Billing Background Service is sleeping for {delay.TotalHours:N2} hours.");
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
