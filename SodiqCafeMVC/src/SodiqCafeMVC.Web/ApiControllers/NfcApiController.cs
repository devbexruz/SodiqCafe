using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SodiqCafeMVC.Web.Hubs;
using System.Threading.Tasks;
using System.Security.Claims;

namespace SodiqCafeMVC.Web.ApiControllers
{
    [Route("api/nfc")]
    [ApiController]
    public class NfcApiController : ControllerBase
    {
        private readonly IHubContext<NfcHub> _hubContext;

        public NfcApiController(IHubContext<NfcHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpGet("scan/{cafeId}/{tableId?}")]
        public async Task<IActionResult> ScanNfc(int cafeId, string? tableId = null)
        {
            // Bu yerda aslida NFC dagi shifrlangan AES/JWT token decrypt qilinib cafeId va tableId aniqlanadi.
            // MVP darsligi uchun to'g'ridan to'g'ri qiymatlar olinmoqda.

            var userId = User.Identity?.IsAuthenticated == true ? 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

            var clientData = new
            {
                CafeId = cafeId,
                TableId = tableId,
                UserId = userId,
                Message = "NFC scanned by customer",
                Timestamp = System.DateTime.UtcNow
            };

            // Shu kafening sotuvchilari ulangan WebSocket guruhiga signal yuborish
            await _hubContext.Clients.Group($"Cafe_{cafeId}").SendAsync("OnNfcScanned", clientData);

            return Ok(new { success = true, message = "Sotuvchiga bildirishnoma yuborildi." });
        }
    }
}
