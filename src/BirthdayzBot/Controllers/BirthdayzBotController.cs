using System.Collections.Generic;
using System.Threading.Tasks;
using BirthdayzBot.Commands;
using BirthdayzBot.Models;
using Microsoft.AspNetCore.Mvc;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using Newtonsoft.Json;

namespace BirthdayzBot.Controllers
{
    public class BirthdayzBotController : Controller
    {
        private readonly TelegramBot _bot;

        public BirthdayzBotController(TelegramBot bot)
        {
            _bot = bot;
        }

        public async Task<IActionResult> Me()
        {
            var me = await _bot.MakeRequestAsync(new GetMe());
            return Json(me, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }

        public async Task<IActionResult> Updates()
        {
            var logs = new List<string>();
            long offset = 0;

            var updates = await _bot.MakeRequestAsync(new GetUpdates() { Offset = offset });
            if (updates == null)
                return Content("No updates");

            foreach (var update in updates)
            {
                offset = update.UpdateId + 1;
                if (update.Message == null)
                    continue;

                logs.Add($"{update.Message.Date:G} >> {update.Message.Chat.Title ?? "NoTitle"} >> {update.Message.From.FirstName} >> {update.Message.Text}");

                if (update.Message.Text == null)
                    continue;

                string args;
                var commandType = BotCommand.ParseCommand(update.Message.Text, out args);
                switch (commandType)
                {
                    case BotCommandType.MyBd:
                        await _bot.MakeRequestAsync(new MyBdCommand().GetResponse(args, update.Message));
                        break;
                    case BotCommandType.Invalid:
                        await _bot.MakeRequestAsync(new SendMessage(update.Message.Chat.Id, "_Invalid command_")
                        {
                            DisableNotification = true,
                            ReplyToMessageId = update.Message.MessageId,
                            ParseMode = SendMessage.ParseModeEnum.Markdown,
                        });
                        break;
                }
            }
            await _bot.MakeRequestAsync(new GetUpdates() { Offset = offset });
            return Json(logs, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }
    }
}
