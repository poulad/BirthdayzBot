using Microsoft.EntityFrameworkCore;

namespace BirthdayzBot.Models
{
    public class BirthdayzContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Chat> Chats { get; set; }

        public DbSet<Birthday> Birthdays { get; set; }

        public BirthdayzContext(DbContextOptions<BirthdayzContext> options)
            : base(options)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Birthday>().HasKey(bd => new { bd.UserId, bd.ChatId });
        }
    }
}
