using System.ComponentModel.DataAnnotations;

namespace SodiqCafeMVC.Domain.Enums
{
    public enum ProductCategory
    {
        [Display(Name = "Ichimliklar")]
        Drinks = 1,
        
        [Display(Name = "Shirinliklar")]
        Desserts = 2,
        
        [Display(Name = "Taomlar")]
        Foods = 3
    }
}
