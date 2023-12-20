using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SocialNetworkSignalR.Entities
{
    public class CustomIdentityDbContext:IdentityDbContext<CustomIdentityUser,CustomIdentityRole,string>
    {
        public CustomIdentityDbContext(DbContextOptions<CustomIdentityDbContext> options)
            :base(options)
        {
            
        }

        public DbSet<Friend>? Friends { get; set; }
        public DbSet<FriendRequest>? FriendRequests { get; set; }
        public DbSet<Message>? Messages { get; set; }
        public DbSet<Chat>? Chats { get; set; }

    }
}
