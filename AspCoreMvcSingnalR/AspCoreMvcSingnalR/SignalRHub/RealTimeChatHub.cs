using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Xml;
using AspCoreMvcSingnalR.DatabaseEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AspCoreMvcSingnalR.SignalRHub
{
    [Authorize]
    public class RealTimeChatHub : Hub
    {
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            using (var _chatDbContext = new ChatDbContext())
            {
                //Get logged UserId From the claim
                Guid userId = new Guid(Context.User.FindFirstValue(ClaimTypes.PrimarySid));
                //Set User status to offline 
                var user = _chatDbContext.Users.FirstOrDefault(a => a.UserId == userId);
                user.IsOnline = true;
                user.DisconnectedAt = DateTime.UtcNow;
                _chatDbContext.SaveChanges();
            }
            await Clients.All.SendAsync("UserStatusChanged", "StateChangedLoadLatestChatList");
            Debug.WriteLine("Client disconnected: " + Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
        public override async Task OnConnectedAsync()
        {
            using (var _chatDbContext = new ChatDbContext())
            {
                //Get logged UserId From the claim
                Guid userId = new Guid(Context.User.FindFirstValue(ClaimTypes.PrimarySid));
                //Set User status to online
                var user = _chatDbContext.Users.FirstOrDefault(a => a.UserId == userId);
                user.IsOnline = true;
                _chatDbContext.SaveChanges();
            }
            await Clients.All.SendAsync("UserStatusChanged", "StateChangedLoadLatestChatList");
            await base.OnConnectedAsync();
        }
        //Create a group for each user to chat separately for private conversations.
        public void CreateUserChatGroup(string userId)
        {
            var id = Context.ConnectionId;
            Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        //Mark message as seen
        public async Task MarkMessageAsSeen(Guid senderUserId)
        {
            using (var _chatDbContext = new ChatDbContext())
            {
                //Set MessageAsSeen
                Guid userId = new Guid(Context.User.FindFirstValue(ClaimTypes.PrimarySid));
                var userChatHistories = _chatDbContext.UserChatHistory.Where(a => a.ReceiverUserId == userId && a.SenderUserId == senderUserId
                && a.SeetAt == null).ToList();
                if (userChatHistories?.Any() ?? false)
                {
                    foreach (var userChat in userChatHistories)
                    {
                        userChat.SeetAt = DateTime.UtcNow;
                        userChat.IsSeen = true;
                    }
                    _chatDbContext.SaveChanges();
                }
            }
        }
        //Send message to SendMessageToUserChatGroup
        public async Task SendMessageToUserChatGroup(string senderUserId, string senderName, string receiverUserId, string message)
        {
            //Insert message to database then send it to the Client
            var _chatDbContext = new ChatDbContext();
            UserChatHistory chatHistory = new UserChatHistory();
            chatHistory.Message = message;
            chatHistory.IsSeen = false;
            chatHistory.CreatedAt = DateTime.UtcNow;
            chatHistory.SenderUserId = new Guid(senderUserId);
            chatHistory.ReceiverUserId = new Guid(receiverUserId);
            await _chatDbContext.UserChatHistory.AddAsync(chatHistory);
            await _chatDbContext.SaveChangesAsync();
            await Clients.Group(receiverUserId).SendAsync("ReceiveMessage",
                new ReceiveMessageDTO
                {
                    Message = message,
                    SenderName = senderName,
                    SenderUserId = senderUserId
                });
        }
    }
    public class ReceiveMessageDTO
    {
        public string Message { get; set; }
        public string SenderUserId { get; set; }
        public string SenderName { get; set; }
    }
}
