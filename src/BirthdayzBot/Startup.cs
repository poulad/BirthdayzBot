﻿using System.IO;
using BirthdayzBot.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;

namespace BirthdayzBot
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        private readonly TelegramBot _bot;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables("BirthdayzBot_")
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            Configuration = builder.Build();

            _bot = new TelegramBot(Configuration["ApiToken"]);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(provider => _bot);
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var botName = Configuration["BotName"];
            var apiToken = Configuration["ApiToken"];

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(botName, apiToken,
                    new
                    {
                        Controller = nameof(BirthdayzBotController).Replace("Controller", ""),
                        Action = nameof(BirthdayzBotController.ProcessUpdate)
                    });
            });
            var info = _bot.MakeRequestAsync(new GetWebhookInfo()).Result;
            var certificate = Configuration.GetValue<string>("Certificate");
            var hostName = Configuration.GetValue<string>("HostName");
            if (Configuration.GetValue<bool>("UseWebhook") && !string.IsNullOrEmpty(certificate) && !string.IsNullOrEmpty(hostName))
            {
                var url = $"https://{hostName}" + $"/{botName}/{apiToken}";
                var isWebhookSet = _bot.MakeRequestAsync(new SetWebhook(url, new FileToSend(new FileStream(certificate, FileMode.Open), "certificate.pem"))).Result;
                // todo : log the result
            }
        }
    }
}
