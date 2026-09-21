using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Domain.Entities;

namespace SodiqCafeMVC.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Cafe> Cafes { get; }
        DbSet<UserSession> Sessions { get; }
        DbSet<Invoice> Invoices { get; }
        DbSet<PaymentTransaction> Transactions { get; }
        DbSet<Product> Products { get; }
        DbSet<CafeUser> CafeUsers { get; }
        DbSet<BonusCampaign> BonusCampaigns { get; }
        DbSet<UserCampaignProgress> UserCampaignProgresses { get; }
        DbSet<UserReward> UserRewards { get; }
        DbSet<ServiceRequest> ServiceRequests { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
