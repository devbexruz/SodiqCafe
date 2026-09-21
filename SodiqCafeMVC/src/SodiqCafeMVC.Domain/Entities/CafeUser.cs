using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SodiqCafeMVC.Domain.Entities
{
    // Mijozlarning kafedagi faolligi (Bonus tizimi uchun)
    public class CafeUser
    {
        [Key]
        public int Id { get; set; }

        public int CafeId { get; set; }
        [ForeignKey(nameof(CafeId))]
        public Cafe Cafe { get; set; } = null!;

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public int CurrentBonusCount { get; set; } = 0; // Hozirgi yig'ilgan bonuslar
        public int TotalBonusesEarned { get; set; } = 0; // Tarix davomida yig'ilgan bonuslar jami

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow; // Oxirgi marta bonus olgan yoki sarflagan vaqti
        

    }
}
