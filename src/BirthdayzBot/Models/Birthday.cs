using System;

namespace BirthdayzBot.Models
{
    public class Birthday
    {
        public DateTime Birthdate { get; set; }

        public long UserId { get; set; }

        public long ChatId { get; set; }

        public User User { get; set; }

        public Chat Chat { get; set; }

        public string UserChatStatus { get; set; }
    }
}
