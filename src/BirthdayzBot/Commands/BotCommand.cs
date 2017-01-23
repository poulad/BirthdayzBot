using System.Linq;
using System.Text.RegularExpressions;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace BirthdayzBot.Commands
{
    public abstract class BotCommand
    {
        public abstract string Name { get; }

        public static string[] CommandNames => new[] { "mybd" };

        public abstract RequestBase<Message> GetResponse(string args, Message message);

        public static BotCommandType ParseCommand(string text, out string args)
        {
            args = string.Empty;
            var tokens = Regex.Split(text.Trim(), @"\s");
            int minCommandLength = CommandNames.Select(commandName => commandName.Length).Min();
            if (!tokens.Any() || tokens[0].Length < minCommandLength)
            {
                return BotCommandType.Invalid;
            }

            var commandText = tokens[0].Substring(1);
            if (commandText.IndexOf('@') != -1)
                commandText = commandText.Substring(0, commandText.IndexOf('@'));

            if (tokens.Length > 1)
                args = tokens.Skip(1).Aggregate((s1, s2) => $"{s1} {s2}");

            switch (commandText.ToLower())
            {
                case "mybd":
                    return BotCommandType.MyBd;
                default:
                    return BotCommandType.Invalid;
            }
        }
    }
}
