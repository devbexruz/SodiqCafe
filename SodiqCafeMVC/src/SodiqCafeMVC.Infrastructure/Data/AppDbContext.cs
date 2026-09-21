using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Domain.Entities;

namespace SodiqCafeMVC.Infrastructure.Data
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Cafe> Cafes => Set<Cafe>();
        public DbSet<UserSession> Sessions => Set<UserSession>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<PaymentTransaction> Transactions => Set<PaymentTransaction>();
        
        public DbSet<Product> Products => Set<Product>();
        public DbSet<CafeUser> CafeUsers => Set<CafeUser>();
        public DbSet<BonusCampaign> BonusCampaigns => Set<BonusCampaign>();
        public DbSet<UserCampaignProgress> UserCampaignProgresses => Set<UserCampaignProgress>();
        public DbSet<UserReward> UserRewards => Set<UserReward>();
        public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
        public DbSet<CafePriceChangeHistory> CafePriceChangeHistories => Set<CafePriceChangeHistory>();
        public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
        public DbSet<DailySnapshot> DailySnapshots => Set<DailySnapshot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User konfiguratsiyasi
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Balance).HasPrecision(18, 2);

                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                // 1 User - 1 Cafe (Kafe egasi)
                entity.HasMany(u => u.Cafes)
                      .WithOne(c => c.Owner)
                      .HasForeignKey(c => c.OwnerId)
                      .OnDelete(DeleteBehavior.SetNull);

                // 1 User - Many Sessions
                entity.HasMany(u => u.Sessions)
                      .WithOne(s => s.User)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cafe konfiguratsiyasi
            modelBuilder.Entity<Cafe>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Address).HasMaxLength(250);
                entity.Property(c => c.PhoneNumber).HasMaxLength(30);
                entity.Property(c => c.BasePrice).HasPrecision(18, 2);
                entity.Property(c => c.PricePerExtraUsers).HasPrecision(18, 2);

                entity.HasMany(c => c.Invoices)
                      .WithOne(i => i.Cafe)
                      .HasForeignKey(i => i.CafeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Transactions)
                      .WithOne(t => t.Cafe)
                      .HasForeignKey(t => t.CafeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserSession konfiguratsiyasi
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.SessionToken).IsRequired().HasMaxLength(128);
                entity.Property(s => s.RefreshToken).IsRequired().HasMaxLength(256);

                entity.HasIndex(s => s.SessionToken).IsUnique();
                entity.HasIndex(s => s.RefreshToken);
                entity.HasIndex(s => s.UserId);
            });

            // Invoice konfiguratsiyasi
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(i => i.Amount).HasPrecision(18, 2);
                entity.Property(i => i.Notes).HasMaxLength(500);

                entity.HasIndex(i => i.InvoiceNumber).IsUnique();
                entity.HasIndex(i => i.CafeId);
                entity.HasIndex(i => i.Status);
            });

            // PaymentTransaction konfiguratsiyasi
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Amount).HasPrecision(18, 2);
                entity.Property(t => t.Description).HasMaxLength(250);

                entity.HasIndex(t => t.CafeId);
            });

            // Product konfiguratsiyasi
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Price).HasPrecision(18, 2);

                entity.HasOne(p => p.Cafe)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CafeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CafeUser konfiguratsiyasi
            modelBuilder.Entity<CafeUser>(entity =>
            {
                entity.HasKey(cu => cu.Id);
                
                // Bitta user bitta kafeda faqat bir marta a'zo bo'ladi
                entity.HasIndex(cu => new { cu.CafeId, cu.UserId }).IsUnique();

                entity.HasOne(cu => cu.Cafe)
                      .WithMany(c => c.CafeUsers)
                      .HasForeignKey(cu => cu.CafeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cu => cu.User)
                      .WithMany(u => u.JoinedCafes)
                      .HasForeignKey(cu => cu.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // BonusCampaign konfiguratsiyasi
            modelBuilder.Entity<BonusCampaign>(entity =>
            {
                entity.HasKey(bc => bc.Id);
                entity.Property(bc => bc.Name).IsRequired().HasMaxLength(150);
                entity.Property(bc => bc.ConditionValue).HasPrecision(18, 2);

                entity.HasOne(bc => bc.Cafe)
                      .WithMany(c => c.BonusCampaigns)
                      .HasForeignKey(bc => bc.CafeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserCampaignProgress konfiguratsiyasi
            modelBuilder.Entity<UserCampaignProgress>(entity =>
            {
                entity.HasKey(ucp => ucp.Id);
                
                entity.HasIndex(ucp => new { ucp.UserId, ucp.BonusCampaignId }).IsUnique();

                entity.HasOne(ucp => ucp.User)
                      .WithMany(u => u.CampaignProgresses)
                      .HasForeignKey(ucp => ucp.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ucp => ucp.BonusCampaign)
                      .WithMany(bc => bc.Progresses)
                      .HasForeignKey(ucp => ucp.BonusCampaignId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserReward konfiguratsiyasi
            modelBuilder.Entity<UserReward>(entity =>
            {
                entity.HasKey(ur => ur.Id);

                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.Rewards)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.BonusCampaign)
                      .WithMany(bc => bc.Rewards)
                      .HasForeignKey(ur => ur.BonusCampaignId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Cafe)
                      .WithMany(c => c.UserRewards)
                      .HasForeignKey(ur => ur.CafeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ServiceRequest konfiguratsiyasi
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(sr => sr.Id);
                
                entity.HasOne(sr => sr.User)
                      .WithMany(u => u.ServiceRequests)
                      .HasForeignKey(sr => sr.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sr => sr.Cafe)
                      .WithMany(c => c.ServiceRequests)
                      .HasForeignKey(sr => sr.CafeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CafePriceChangeHistory konfiguratsiyasi
            modelBuilder.Entity<CafePriceChangeHistory>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.OldPrice).HasPrecision(18, 2);
                entity.Property(h => h.NewPrice).HasPrecision(18, 2);
                entity.Property(h => h.Reason).IsRequired().HasMaxLength(500);

                entity.HasOne(h => h.Cafe)
                      .WithMany(c => c.PriceChangeHistories)
                      .HasForeignKey(h => h.CafeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SystemSettings seed
            modelBuilder.Entity<SystemSettings>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.BasePrice).HasPrecision(18, 2);
                entity.Property(s => s.PricePerExtraUsers).HasPrecision(18, 2);

                entity.HasData(new SystemSettings
                {
                    Id = 1,
                    BasePrice = 120000m,
                    BaseUsersLimit = 500,
                    PricePerExtraUsers = 12000m,
                    ExtraUsersStep = 100,
                    UpdatedAt = new System.DateTime(2023, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
                });
            });
        }
    }
}
