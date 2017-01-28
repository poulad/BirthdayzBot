using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BirthdayzBot.Commands;
using Microsoft.AspNetCore.Mvc;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using NetTelegramBotApi.Util;
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

                await RespondUpdate(update);
            }
            await _bot.MakeRequestAsync(new GetUpdates() { Offset = offset });
            return Json(logs, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }

        [HttpPost]
        public async Task<IActionResult> ProcessUpdate([FromBody]object data)
        {
            var update = ParseUpdate(data);
            if (update == null)
            {
                return BadRequest();
            }

            await RespondUpdate(update);

            return Ok();
        }

        private async Task RespondUpdate(Update update)
        {
            if (update.Message == null || update.Message.Text == null)
                return;

            var command = BaseBotCommand.ParseCommand(update);
            if (command == null)
                await _bot.MakeRequestAsync(new SendMessage(update.Message.Chat.Id, "_Invalid command_")
                {
                    DisableNotification = true,
                    ReplyToMessageId = update.Message.MessageId,
                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                });
            else
                await _bot.MakeRequestAsync(command.GetResponse());
        }

        private static Update ParseUpdate(object data)
        {
            if (data == null)
                return null;

            Update update;
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
                Converters = new List<JsonConverter>() { new UnixDateTimeConverter() }
            };
            try
            {
                update = JsonConvert.DeserializeObject<Update>(data.ToString(), settings);
            }
            catch (Exception)
            {
                // todo: log
                update = null;
            }
            return update;
        }
    }
}
