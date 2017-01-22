namespace BirthdayzBot.Models
{
    public class Chat
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string ChatType { get; set; }

        public bool AllMembersAdmin { get; set; }
    }
}
