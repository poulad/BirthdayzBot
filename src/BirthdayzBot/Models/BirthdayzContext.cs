using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BirthdayzBot.Models
{
    public class BirthdayzContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Chat> Chats { get; set; }

        public DbSet<Birthday> Birthdays { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(System.IO.Directory.GetCurrentDirectory())
               .AddEnvironmentVariables("Birthdayz_")
               .AddJsonFile("config.json", true)
               .Build();

            var conn = config["BirthdayzConnectionString"];

            optionsBuilder.UseNpgsql(conn);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Birthday>().HasKey(bd => new { bd.UserId, bd.ChatId });
        }
    }
}
