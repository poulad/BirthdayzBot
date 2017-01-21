using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using Microsoft.Extensions.Configuration;
using BirthdayzBot.Models;

namespace BirthdayzBot
{
    public class Program
    {
        private static bool _stopMe;
        private static string _botName;

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddEnvironmentVariables("Birthdayz_")
                .AddJsonFile("config.json", true)
                .Build();
            _botName = config["BotName"];
            var accessToken = config["AccessToken"];

            Console.WriteLine("Starting your bot...");
            Console.WriteLine();

            var t = Task.Run(() => RunBot(accessToken));
            t.ContinueWith(task => { Console.WriteLine(t.Exception?.GetBaseException()); });

            Console.ReadLine();

            _stopMe = true;
        }

        public static void RunBot(string accessToken)
        {
            var bot = new TelegramBot(accessToken);

            var me = bot.MakeRequestAsync(new GetMe()).Result;

            if (me == null)
            {
                Console.WriteLine("GetMe() FAILED. Do you forget to add your AccessToken to config.json?");
                Console.WriteLine("(Press ENTER to quit)");
                Console.ReadLine();
                return;
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("{0} (@{1}) connected!", me.FirstName, me.Username);

            Console.WriteLine();
            Console.WriteLine("Find @{0} in Telegram and send him a message - it will be displayed here", me.Username);
            Console.WriteLine("(Press ENTER to stop listening and quit)");
            Console.WriteLine();
            long offset = 0;
            while (!_stopMe)
            {
                var updates = bot.MakeRequestAsync(new GetUpdates() { Offset = offset }).Result;
                if (updates == null)
                    continue;

                foreach (var update in updates)
                {
                    offset = update.UpdateId + 1;
                    if (update.Message == null)
                    {
                        continue;
                    }



                    var from = update.Message.From;
                    var text = update.Message.Text;
                    Console.WriteLine(
                        "Msg from {0}[{5}] {1} ({2}) at {4}: {3}",
                        @from.FirstName,
                        @from.LastName,
                        @from.Username,
                        text,
                        update.Message.Date,
                        from.Id);

                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var dbContext = new BirthdayzContext();

                    #region Getting User Info

                    var user = dbContext.Users.FirstOrDefault(u => u.Id == @from.Id);
                    if (user == null)
                    {
                        user = new User()
                        {
                            Id = from.Id,
                            FirstName = from.FirstName,
                            LastName = from.LastName,
                            UserName = from.Username,
                        };
                        dbContext.Users.Add(user);
                    }

                    #endregion

                    #region Getting Chat Info

                    var chat = dbContext.Chats.FirstOrDefault(c => c.Id == update.Message.Chat.Id);
                    if (chat == null)
                    {
                        chat = new Chat()
                        {
                            Id = update.Message.Chat.Id,
                            Title = update.Message.Chat.Title,
                            ChatType = update.Message.Chat.Type,
                            AllMembersAdmin = update.Message.Chat.AllMembersAreAdministrators,
                        };
                        dbContext.Chats.Add(chat);
                    }


                    #endregion

                    var birthday = dbContext.Birthdays.FirstOrDefault(b => b.UserId == user.Id && b.ChatId == chat.Id);

                    string param;
                    var commandType = ParseCommandType(text, out param);

                    switch (commandType)
                    {
                        #region MyBd Command

                        case CommandType.MyBirthDate:
                            DateTime birthDate;

                            if (string.IsNullOrEmpty(param))
                            {
                                if (birthday != null)
                                {
                                    birthDate = birthday.Birthdate;
                                    var messageText = $"*{birthDate:MMMM}, {birthDate:dd}* is your birthday!";
                                    bot.MakeRequestAsync(new SendMessage(chat.Id, messageText)
                                    {
                                        DisableNotification = true,
                                        ReplyToMessageId = update.Message.MessageId,
                                        ParseMode = SendMessage.ParseModeEnum.Markdown,
                                    }).Wait();
                                }
                                else
                                {
                                    bot.MakeRequestAsync(new SendMessage(chat.Id,
                                        "You never told me your `bd` in this chat :/")
                                    {
                                        DisableNotification = true,
                                        ReplyToMessageId = update.Message.MessageId,
                                        ParseMode = SendMessage.ParseModeEnum.Markdown,
                                    }).Wait();
                                }
                            }
                            else if (DateTime.TryParse(param, out birthDate))
                            {
                                birthday = birthday ?? new Birthday();
                                birthday.User = user;
                                birthday.Chat = chat;
                                birthday.Birthdate = birthDate;
                                dbContext.Birthdays.Add(birthday);
                                dbContext.SaveChanges();
                                var messageText = $"*{birthDate:MMMM}, {birthDate:dd}*. Got it!";
                                bot.MakeRequestAsync(new SendMessage(chat.Id, messageText)
                                {
                                    DisableNotification = true,
                                    ReplyToMessageId = update.Message.MessageId,
                                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                                }).Wait();
                            }
                            else
                            {
                                bot.MakeRequestAsync(new SendMessage(chat.Id, $"Not a valid date: _{param}_")
                                {
                                    DisableNotification = true,
                                    ReplyToMessageId = update.Message.MessageId,
                                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                                }).Wait();
                            }
                            break;

                        #endregion

                        #region Invalid Command

                        default:
                            bot.MakeRequestAsync(new SendMessage(chat.Id, "_Invalid command_")
                            {
                                DisableNotification = true,
                                ReplyToMessageId = update.Message.MessageId,
                                ParseMode = SendMessage.ParseModeEnum.Markdown,
                            }).Wait();
                            break;

                            #endregion

                    }

                    dbContext.Dispose();
                }
            }
        }

        public static CommandType ParseCommandType(string command, out string param)
        {
            CommandType commandType;
            param = string.Empty;
            var tokens = Regex.Split(command.Trim(), @"\s");
            if (!tokens.Any() || tokens[0].Length < 3)
            {
                return CommandType.Invalid;
            }
            if (tokens[0].ToLower().EndsWith('@' + _botName.ToLower()))
            {
                tokens[0] = tokens[0].Substring(0, tokens[0].LastIndexOf('@'));
            }
            switch (tokens[0].ToLower().Substring(1))
            {
                case "mybd":
                    commandType = CommandType.MyBirthDate;
                    if (tokens.Length == 2)
                        param = tokens[1];
                    else if (tokens.Length > 2)
                        commandType = CommandType.Invalid;
                    break;
                case "bd":
                    if (tokens.Length == 2)
                    {
                        commandType = CommandType.BirthDate;
                        param = tokens[1];
                    }
                    else
                        commandType = CommandType.Invalid;
                    break;
                case "f1":
                    commandType = CommandType.F1;
                    break;
                default:
                    commandType = CommandType.Invalid;
                    break;
            }
            return commandType;
        }
    }
}
