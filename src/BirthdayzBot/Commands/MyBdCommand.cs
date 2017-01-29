using System;
using System.Linq;
using System.Text.RegularExpressions;
using BirthdayzBot.Models;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace BirthdayzBot.Commands
{
    public class MyBdCommand : BaseBotCommand
    {
        private readonly BirthdayzContext _dbContext;

        private Models.User _user;

        private Models.Chat _chat;

        public override string Name => _commandName;

        private static readonly string _commandName = "mybd";

        public MyBdCommand(Update update, string args = null)
            : base(update, args)
        {
            _dbContext = new BirthdayzContext();
        }

        public override RequestBase<Message> GetResponse()
        {
            string responseText;
            EnsureUser(Update.Message);
            EnsureChat(Update.Message);
            var birthday = _dbContext.Birthdays.FirstOrDefault(x => x.ChatId == _chat.Id && x.UserId == _user.Id);

            if (string.IsNullOrWhiteSpace(Args))
            {
                if (birthday == null)
                {
                    responseText = "You never told me your `bd` in this chat :/";
                }
                else
                {
                    var birthDate = birthday.Birthdate;
                    responseText = $"*{birthDate:MMMM}, {birthDate:dd}* is your birthday!";
                }
            }
            else
            {
                var birthDate = ParseDate(Args);
                if (birthDate.Equals(new DateTime(1, 1, 1)))
                {
                    responseText = "Not a valid date. Try it like `jan 1 91` or `jan 1`";
                }
                else
                {
                    if (birthday == null)
                    {
                        birthday = new Birthday()
                        {
                            User = _user,
                            Chat = _chat
                        };
                        _dbContext.Birthdays.Add(birthday);
                    }
                    birthday.Birthdate = birthDate;
                    var formattedDate = birthDate.Year == 1 ? $"{birthDate:MMMM}, {birthDate:dd}" : $"{birthDate:MMMM} {birthDate:dd}, {birthDate.Year}";
                    responseText = $"*{formattedDate}*. Got it!";
                }
            }
            _dbContext.SaveChanges();
            return new SendMessage(Update.Message.Chat.Id, responseText)
            {
                DisableNotification = true,
                ReplyToMessageId = Update.Message.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
            };
        }

        private void EnsureUser(Message message)
        {
            _user = _dbContext.Users.FirstOrDefault(x => x.Id == message.From.Id);
            if (_user != null)
                return;

            _user = new Models.User()
            {
                Id = message.From.Id,
                FirstName = message.From.FirstName,
                UserName = message.From.Username,
                LastName = message.From.LastName
            };
            _dbContext.Users.Add(_user);
        }

        private void EnsureChat(Message message)
        {
            _chat = _dbContext.Chats.FirstOrDefault(x => x.Id == message.Chat.Id);
            if (_chat != null)
                return;

            _chat = new Models.Chat()
            {
                Id = message.Chat.Id,
                Title = message.Chat.Title,
                AllMembersAdmin = message.Chat.AllMembersAreAdministrators,
                ChatType = message.Chat.Type
            };
            _dbContext.Chats.Add(_chat);
        }

        /// <summary>
        /// Converts the input into a date
        /// </summary>
        /// <param name="input"></param>
        /// <returns>"1/1/1" if input is not valid. If input doesn't have year, Year will be 1</returns>
        private static DateTime ParseDate(string input)
        {
            var date = new DateTime(1, 1, 1);
            var match = Regex.Match(input, @"([a-z]{3})\s+(\d{1,2})(?:\s+(\d{2,4}))?", RegexOptions.IgnoreCase);
            if (!match.Success)
                return date;

            var day = int.Parse(match.Groups[2].Value);
            var year = string.IsNullOrEmpty(match.Groups[3].Value) ? -1 : int.Parse(match.Groups[3].Value);
            var yearStr = year == -1 ? "0001" : $"{year:00}";
            var formattedDate = $"{match.Groups[1].Value} {day:00} {yearStr}";
            DateTime.TryParse(formattedDate, out date);
            return date;
        }
    }
}
