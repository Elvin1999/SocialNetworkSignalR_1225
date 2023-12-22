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

            var myrequests = _context.FriendRequests.Where(r => r.SenderId == user.Id);

            var myfriends = await _context.Friends.Where(f => f.OwnId == user.Id || f.YourFriendId == user.Id).ToListAsync();
            foreach (var item in users)
            {
                var request = myrequests.FirstOrDefault(r => r.ReceiverId == item.Id && r.Status == "Request");
                if (request != null)
                {
                    item.HasRequestPending = true;
                }
                var friend = myfriends.FirstOrDefault(f => f.OwnId == item.Id || f.YourFriendId == item.Id);
                if (friend != null)
                {
                    item.IsFriend = true;
                }
            }

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

        public async Task<IActionResult> GetMyFriends()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var friends = await _context.Friends
                .Include(nameof(Friend.YourFriend)).ToListAsync();
            var myfriends = friends.Where(f => f.OwnId == user.Id);
            return Ok(myfriends);
        }

        public async Task<IActionResult> UnFollow(string id)
        {
            try
            {
                var current = await _userManager.GetUserAsync(HttpContext.User);
                var friendItems = _context.Friends.Where(f => f.OwnId == id && f.YourFriendId == current.Id || f.YourFriendId == id && f.OwnId == current.Id);
                _context.Friends.RemoveRange(friendItems);
                await _context.SaveChangesAsync();
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> TakeRequest(string id)
        {
            try
            {
                var current = await _userManager.GetUserAsync(HttpContext.User);
                var request = await _context.FriendRequests.FirstOrDefaultAsync(f => f.ReceiverId == id && f.SenderId == current.Id);
                _context.FriendRequests.Remove(request);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> DeclineRequest(int id, string senderId)
        {
            try
            {
                var current = await _userManager.GetUserAsync(HttpContext.User);
                var request = await _context.FriendRequests.FirstOrDefaultAsync(f => f.Id == id);
                var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == senderId);

                _context.FriendRequests.Add(new FriendRequest
                {
                    Content = $"${current.UserName} declined your friend request at {DateTime.Now.ToLongTimeString()}",
                    SenderId = current.Id,
                    Sender = current,
                    ReceiverId = sender.Id,
                    Status = "Notification"
                });


                _context.FriendRequests.Remove(request);
                await _context.SaveChangesAsync();
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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

        public async Task<IActionResult> GoChat(string id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.SenderId == user.Id && c.ReceiverId == id
            || c.SenderId == id && c.ReceiverId == user.Id);

            if (chat == null)
            {
                chat = new Chat
                {
                    Messages=new List<Message>(),
                     ReceiverId=id,
                      SenderId=user.Id
                };

                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
            }
            var chats = await _context.Chats.Include(nameof(Chat.Receiver)).Where(c => c.SenderId == user.Id || c.ReceiverId == user.Id).ToListAsync();

            foreach (var item in chats)
            {
                if (item.ReceiverId == user.Id)
                {
                    item.Receiver = _context.Users.FirstOrDefault(u => u.Id == item.SenderId);
                }
            }

            var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            List<Message> messages = new List<Message>();
            if (chat != null)
            {
                messages = await _context.Messages.Where(m => m.ChatId == chat.Id).OrderBy(m=>m.DateTime).ToListAsync();
            }

            chat.Messages=messages;

            var model = new ChatViewModel
            {
                CurrentChat = chat,
                Chats = chats
            };

            return View(model); 
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