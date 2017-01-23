using System;
using System.Linq;
using BirthdayzBot.Models;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace BirthdayzBot.Commands
{
    public class MyBdCommand : BotCommand
    {
        private readonly BirthdayzContext _dbContext;

        private Models.User _user;

        private Models.Chat _chat;

        public override string Name => "mybd";

        public MyBdCommand()
        {
            _dbContext = new BirthdayzContext();
        }

        public override RequestBase<Message> GetResponse(string args, Message message)
        {
            string responseText = "";
            EnsureUser(message);
            EnsureChat(message);
            var birthday = _dbContext.Birthdays.FirstOrDefault(x => x.ChatId == _chat.Id && x.UserId == _user.Id);

            if (string.IsNullOrWhiteSpace(args))
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
                DateTime birthDate;
                if (DateTime.TryParse(args, out birthDate))
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

                    if (_dbContext.Birthdays.Contains(birthday))
                        responseText = $"*{birthDate:MMMM}, {birthDate:dd}*. Got it!";
                }
                else
                {
                    responseText = $"Not a valid date: _{args}_";
                }
            }
            _dbContext.SaveChanges();
            return new SendMessage(message.Chat.Id, responseText)
            {
                DisableNotification = true,
                ReplyToMessageId = message.MessageId,
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
    }
}
