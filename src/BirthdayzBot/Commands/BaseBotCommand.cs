using System.Linq;
using System.Text.RegularExpressions;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace BirthdayzBot.Commands
{
    public abstract class BaseBotCommand
    {
        public abstract string Name { get; }

        public static string[] CommandNames => new[] { "mybd" };

        public Update Update { get; protected set; }

        protected string Args { get; set; }


        protected BaseBotCommand(Update update, string args = null)
        {
            Update = update;
            Args = args ?? "";
        }

        public abstract RequestBase<Message> GetResponse();

        public static BaseBotCommand ParseCommand(Update update)
        {
            BaseBotCommand command;
            var args = string.Empty;
            var tokens = Regex.Split(update.Message.Text.Trim(), @"\s");
            var minCommandLength = CommandNames.Select(commandName => commandName.Length).Min();
            if (!tokens.Any() || tokens[0].Length < minCommandLength)
            {
                return null;
            }

            var commandText = tokens[0].Substring(1);
            if (commandText.IndexOf('@') != -1)
                commandText = commandText.Substring(0, commandText.IndexOf('@'));

            if (tokens.Length > 1)
                args = tokens.Skip(1).Aggregate((s1, s2) => $"{s1} {s2}");

            switch (commandText.ToLower())
            {
                case "mybd":
                    command = new MyBdCommand(update, args);
                    break;
                case "bdz":
                    command = new BdzCommand(update);
                    break;
                default:
                    command = null;
                    break;
            }
            return command;
        }
    }
}
