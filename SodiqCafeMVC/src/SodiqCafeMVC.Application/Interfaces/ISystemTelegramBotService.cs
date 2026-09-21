using System.Threading.Tasks;

namespace SodiqCafeMVC.Application.Interfaces
{
    public interface ISystemTelegramBotService
    {
        Task SendInvoiceForApprovalAsync(int invoiceId, string cafeName, decimal amount, string receiptUrl, string invoiceNumber);
        Task SendMessageAsync(string message);
    }
}
