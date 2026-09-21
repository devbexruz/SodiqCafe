using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using SodiqCafeMVC.Infrastructure.Data;

namespace SodiqCafeMVC.Web.Hubs
{
    [Authorize]
    public class NfcHub : Hub
    {
        private readonly AppDbContext _context;

        public NfcHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task JoinCafeGroup(int cafeId)
        {
            var groupName = $"Cafe_{cafeId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinedGroup", groupName);
        }

        public async Task LeaveCafeGroup(int cafeId)
        {
            var groupName = $"Cafe_{cafeId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("LeftGroup", groupName);
        }

        // Token orqali tekshirish
        public async Task JoinOrderGroup(int orderId, string token)
        {
            var request = await _context.ServiceRequests.FindAsync(orderId);
            if (request != null && request.Token == token)
            {
                var groupName = $"Order_{orderId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            else
            {
                throw new HubException("Invalid token for this order.");
            }
        }

        public async Task LeaveOrderGroup(int orderId)
        {
            var groupName = $"Order_{orderId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
