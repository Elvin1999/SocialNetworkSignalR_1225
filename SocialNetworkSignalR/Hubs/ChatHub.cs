using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SocialNetworkSignalR.Entities;

namespace SocialNetworkSignalR.Hubs
{
    public class ChatHub:Hub
    {
        private UserManager<CustomIdentityUser> _userManager;
        private IHttpContextAccessor _contextAccessor;
        private CustomIdentityDbContext _context;

        public ChatHub(UserManager<CustomIdentityUser> userManager, IHttpContextAccessor contextAccessor, CustomIdentityDbContext context)
        {
            _userManager = userManager;
            _contextAccessor = contextAccessor;
            _context = context;
        }

        public async override Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
            var userItem=_context.Users.SingleOrDefault(x=>x.Id == user.Id);
            userItem.IsOnline = true;
            await _context.SaveChangesAsync();

            string info = user.UserName + " connected successfully";
            await Clients.Others.SendAsync("Connect",info);
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
            var userItem = _context.Users.SingleOrDefault(x => x.Id == user.Id);
            userItem.IsOnline = false;
            userItem.DisconnectTime = DateTime.Now;
            await _context.SaveChangesAsync();
            string info = user.UserName + " disconnected successfully";
            await Clients.Others.SendAsync("Disconnect", info);
        }

        public async Task SendFollow(string id)
        {
            await Clients.Users(new String[] { id }).SendAsync("ReceiveNotification");
        }

        public async Task GetMessages(string receiverId,string senderId)
        {
            await Clients.Users(new String[] { receiverId,senderId }).SendAsync("ReceiveMessages");
        }
    }
}
