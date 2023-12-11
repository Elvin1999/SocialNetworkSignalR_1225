using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkSignalR.Entities;
using SocialNetworkSignalR.Helpers;
using SocialNetworkSignalR.Models;

namespace SocialNetworkSignalR.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<CustomIdentityUser> _userManager;
        private RoleManager<CustomIdentityRole> _roleManager;
        private SignInManager<CustomIdentityUser> _signInManager;
        private IWebHostEnvironment _webHost;
        private CustomIdentityDbContext _context;

        public AccountController(UserManager<CustomIdentityUser> userManager,
            RoleManager<CustomIdentityRole> roleManager,
            SignInManager<CustomIdentityUser> signInManager,
            IWebHostEnvironment webHost,
            CustomIdentityDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _webHost = webHost;
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var helper = new ImageHelper(_webHost);
                if (model.File != null)
                {
                    model.ImageUrl = await helper.SaveFile(model.File);
                }

                CustomIdentityUser user = new CustomIdentityUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    ImageUrl = model.ImageUrl,
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        CustomIdentityRole role = new CustomIdentityRole
                        {
                            Name = "Admin"
                        };

                        IdentityResult roleResult = await _roleManager.CreateAsync(role);
                        if (!roleResult.Succeeded)
                        {
                            ModelState.AddModelError("", "We can not add the role!");
                            return View(model);
                        }
                    }

                    _userManager.AddToRoleAsync(user,"Admin").Wait();
                    return RedirectToAction("Login", "Account");

                }
            }

            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result=await _signInManager.PasswordSignInAsync(model.Username, model.Password,model.RememberMe,false);
                if(result.Succeeded)
                {
                    var user=_context.Users.SingleOrDefault(u=>u.UserName==model.Username);
                    if (user != null)
                    {
                        user.ConnectTime=DateTime.Now.ToLongDateString()+" "+DateTime.Now.ToLongTimeString();   
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid Login");
            }
            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
            var user=await _userManager.GetUserAsync(HttpContext.User);
            user.DisconnectTime = DateTime.Now;
            user.IsOnline = false;
            await _context.SaveChangesAsync();
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
