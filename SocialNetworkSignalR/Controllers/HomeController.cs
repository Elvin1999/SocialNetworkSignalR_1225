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
            var user=await _userManager.GetUserAsync(HttpContext.User);
            ViewBag.User = user;    
            return View();
        }

        public async Task<IActionResult> GetAllUsers()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var users = await _context.Users
                .Where(u => u.Id != user.Id)
                .OrderByDescending(u=>u.IsOnline)
                .ToListAsync();

            return Ok(users);
        }

        public async Task<IActionResult> SendFollow(string id)
        {
            var sender=await _userManager.GetUserAsync (HttpContext.User);
            var receiverUser=_userManager.Users.FirstOrDefault(u=>u.Id == id);
            if(receiverUser != null)
            {
                receiverUser.FriendRequests.Add(new FriendRequest
                {
                    Content=$"{sender.UserName} send friend request at {DateTime.Now.ToLongDateString()}",
                    SenderId = sender.Id,
                    Sender=sender,
                    ReceiverId=id,
                    Status="Request"
                });

                await _userManager.UpdateAsync(receiverUser);
                return Ok();
            }
            return BadRequest();
        } 


        public async Task<IActionResult> GetAllRequests()
        {
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var users = _context.Users.Include(nameof(CustomIdentityUser.FriendRequests));
            var user = users.SingleOrDefault(u => u.Id == current.Id);
            var items=user.FriendRequests.Where(r=>r.ReceiverId == user.Id);
            return Ok(items);
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