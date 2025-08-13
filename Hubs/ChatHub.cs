using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

//DENEME AMAÇLI ÇALIŞMIYOR

namespace API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
        }
    }
}
