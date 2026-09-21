namespace SodiqCafeMVC.Domain.Enums
{
    public enum InvoiceStatus
    {
        Pending = 1,     // To'lov kutilmoqda
        Paid = 2,        // To'langan
        Overdue = 3,     // Muddati o'tgan
        Cancelled = 4    // Bekor qilingan
    }

    public enum PaymentType
    {
        TopUp = 1,          // Balans to'ldirish (kirim)
        AutoDeduction = 2,  // Invoys bo'yicha avtomatik yechilish (chiqim)
        ManualAdjustment = 3 // Admin tomonidan tuzatish
    }
}
