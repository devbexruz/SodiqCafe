using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Infrastructure.Services;

namespace SodiqCafeMVC.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=sodiqcafe.db";

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IFileStorageService, CloudflareR2StorageService>();

            // Telegram Bot (Singleton and HostedService)
            services.AddSingleton<SystemTelegramBotService>();
            services.AddSingleton<ISystemTelegramBotService>(provider => provider.GetRequiredService<SystemTelegramBotService>());
            services.AddSingleton<ICafeTelegramBotClientManager, CafeTelegramBotClientManager>();
            services.AddScoped<ICafeTelegramBotService, CafeTelegramBotService>();
            services.AddScoped<ISetTelegramBotWebhook, SetTelegramBotWebhookService>();
            

            // Billing Cron Job
            services.AddHostedService<BillingBackgroundService>();

            // HttpClient for Telegram Bot
            services.AddHttpClient();

            return services;
        }
    }
}
