using System;
using System.Collections.Generic;
using SodiqCafeMVC.Domain.Enums;

namespace SodiqCafeMVC.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Role Role { get; set; } = Role.User;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Barcha kafelar uchun umumiy balans
        public decimal Balance { get; set; } = 0;

        // Agar foydalanuvchi kafe egasi bo'lsa
        public ICollection<Cafe> Cafes { get; set; } = new List<Cafe>();

        // Foydalanuvchining barcha sessiyalari
        public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

        // Mijoz sifatidagi kafalari (Bonuslar uchun)
        public ICollection<CafeUser> JoinedCafes { get; set; } = new List<CafeUser>();

        // Mijozning yig'gan bonus progresslari
        public ICollection<UserCampaignProgress> CampaignProgresses { get; set; } = new List<UserCampaignProgress>();

        // Mijozning yutgan mukofotlari
        public ICollection<UserReward> Rewards { get; set; } = new List<UserReward>();

        // Mijozning navbatlari (Buyurtmalar/So'rovlar)
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}