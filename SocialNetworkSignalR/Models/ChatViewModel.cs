using SocialNetworkSignalR.Entities;

namespace SocialNetworkSignalR
{
    public class ChatViewModel
    {
        public Chat? CurrentChat { get; set; }
        public List<Chat>? Chats { get; set; }
    }
}