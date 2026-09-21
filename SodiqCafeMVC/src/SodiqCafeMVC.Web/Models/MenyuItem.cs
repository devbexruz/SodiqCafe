namespace SodiqCafeMVC.Web.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int CafeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Ichimliklar", "Shirinliklar", "Taomlar"
        public string IconEmoji { get; set; } = "☕"; // Katta iconka uchun
    }
}