using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkSignalR.Entities;
using SocialNetworkSignalR.Models;
using System.Diagnostics;

namespace SocialNetworkSignalR.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private UserManager<CustomIdentityUser> _userManager;
        private CustomIdentityDbContext _context;

        public HomeController(UserManager<CustomIdentityUser> userManager, CustomIdentityDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            ViewBag.User = user;
            return View();
        }

        public async Task<IActionResult> GetAllUsers()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var users = await _context.Users
                .Where(u => u.Id != user.Id)
                .OrderByDescending(u => u.IsOnline)
                .ToListAsync();

            return Ok(users);
        }

        public async Task<IActionResult> SendFollow(string id)
        {
            var sender = await _userManager.GetUserAsync(HttpContext.User);
            var receiverUser = _userManager.Users.FirstOrDefault(u => u.Id == id);
            if (receiverUser != null)
            {
                _context.FriendRequests.Add(new FriendRequest
                {
                    Content = $"{sender.UserName} send friend request at {DateTime.Now.ToLongDateString()}",
                    SenderId = sender.Id,
                    Sender = sender,
                    ReceiverId = id,
                    Status = "Request"
                });
                await _context.SaveChangesAsync();
                await _userManager.UpdateAsync(receiverUser);
                return Ok();
            }
            return BadRequest();
        }

        public async Task<IActionResult> DeleteRequest(int requestId)
        {
            var item = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId);
            if (item != null)
            {
                _context.FriendRequests.Remove(item);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        public async Task<IActionResult> GetAllRequests()
        {
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var requests = _context.FriendRequests.Where(r => r.ReceiverId == current.Id);
            return Ok(requests);
        }

        public async Task<IActionResult> AcceptRequest(string userId, string senderId, int requestId)
        {
            var receiverUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var sender = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == senderId);

            if (receiverUser != null)
            {
                receiverUser.FriendRequests.Add(new FriendRequest
                {
                    Content = $"{sender.UserName} accepted friend request at ${DateTime.Now.ToLongDateString()}",
                    SenderId = sender.Id,
                    Sender = sender,
                    ReceiverId = receiverUser.Id,
                    Status = "Notification"
                });

                var receiverFriend = new Friend
                {
                    OwnId = receiverUser.Id,
                    YourFriendId = sender?.Id,
                };

                var senderFriend = new Friend
                {
                    OwnId = sender.Id,
                    YourFriendId = receiverUser.Id,
                };

                _context.Friends.Add(senderFriend);
                _context.Friends.Add(receiverFriend);

                var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId);
                _context.FriendRequests.Remove(request);

                await _userManager.UpdateAsync(receiverUser);
                await _userManager.UpdateAsync(sender);


                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        public IActionResult Privacy()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}