using AspCoreMvcSingnalR.DatabaseEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspCoreMvcSingnalR.Controllers
{
    [Authorize]
    public class UserChatController : Controller
    {

        ChatDbContext _chatDbContext;
        public UserChatController(ChatDbContext chatDbContext)
        {
            _chatDbContext = chatDbContext;
        }
        public IActionResult Chat()
        {
            User loggedInUser = new User();
            //Get logged in user detail from Claim
            Guid userId = new Guid(User.FindFirstValue(ClaimTypes.PrimarySid));
            string name = User.FindFirstValue(ClaimTypes.Name);
            loggedInUser = new User { UserId = userId, FullName = name };
            return View(loggedInUser);
        }
        public ActionResult ChatList()
        {
            List<User> users = new List<User>();
            Guid userId = new Guid(User.FindFirstValue(ClaimTypes.PrimarySid));
            users = _chatDbContext.Users.Where(a => a.UserId != userId).ToList();
            return PartialView("_ChatList", users);
        }
        public ActionResult GetChatCobversion(Guid userIdToLoadChat)
        {
            ChatConversionModel chatConversion = new ChatConversionModel();
            Guid loginUserId = new Guid(User.FindFirstValue(ClaimTypes.PrimarySid));

            chatConversion.ChatUser = _chatDbContext.Users.FirstOrDefault(a => a.UserId == userIdToLoadChat);

            chatConversion.UserChatHistories = _chatDbContext.UserChatHistory.Include("SenderUser")
                    .Include("ReceiverUser").Where(a => (a.ReceiverUserId == loginUserId && a.SenderUserId == userIdToLoadChat)
                   || (a.ReceiverUserId == userIdToLoadChat && a.SenderUserId == loginUserId)).OrderByDescending(a => a.CreatedAt).ToList();
            ViewData["loginUserId"] = loginUserId;
            return PartialView("_ChatConversion", chatConversion);
        }
    }
    public class ChatConversionModel
    {
        public User? ChatUser { get; set; }  
        public List<UserChatHistory>? UserChatHistories { get; set; }
    }
}
