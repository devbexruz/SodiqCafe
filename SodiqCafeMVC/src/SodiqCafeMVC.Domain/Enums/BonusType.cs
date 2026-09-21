using System.ComponentModel.DataAnnotations;

namespace SodiqCafeMVC.Domain.Enums
{
    public enum BonusType
    {
        [Display(Name = "Katakchalar (Stamp)")]
        StampCard = 1,
        
        [Display(Name = "N-chi mijoz")]
        MilestoneCustomer = 2,
        
        [Display(Name = "Qaytib kelgan mijoz")]
        ReturningCustomer = 3,
        
        [Display(Name = "Yangi mijoz")]
        NewCustomer = 4,
        
        [Display(Name = "Minimal xarid summasi")]
        AmountBased = 5
    }
}
