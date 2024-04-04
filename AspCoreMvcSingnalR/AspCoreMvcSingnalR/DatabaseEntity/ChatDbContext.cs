using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AspCoreMvcSingnalR.DatabaseEntity
{
    public class ChatDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserChatHistory> UserChatHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-MFLCOI2;Initial Catalog=OnlineChatDb;User ID=sa;Password=adk@1234;Encrypt=false;");
        }
    }
    public class User
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? DisconnectedAt { get; set; }
    }
    [Table("UserChatHistory", Schema = "dbo")]
    public class UserChatHistory
    {
        public Guid Id { get; set; }
        public virtual User SenderUser { get; set; }
        public Guid SenderUserId { get; set; }
        public virtual User ReceiverUser { get; set; }
        public Guid ReceiverUserId { get; set; }
        public string Message { get; set; }
        public bool IsSeen { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SeetAt { get; set; }
    }
}
