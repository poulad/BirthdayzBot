using System.IO;
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
            var webhookPath = Configuration["BotName"] + Configuration["ApiToken"];

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(Configuration["BotName"], webhookPath,
                    new
                    {
                        Controller = nameof(BirthdayzBotController).Replace("Controller", ""),
                        Action = nameof(BirthdayzBotController.ProcessUpdate)
                    });
            });

            var useWebhook = Configuration.GetValue<bool>("UseWebhook");
            var certificate = Configuration.GetValue<string>("Certificate");
            var hostName = Configuration.GetValue<string>("HostName");
            if (useWebhook && !string.IsNullOrEmpty(certificate) && !string.IsNullOrEmpty(hostName))
            {
                var webhookInfo = _bot.MakeRequestAsync(new GetWebhookInfo()).Result;
                if (webhookInfo.Url == null)
                {
                    var isWebhookSet = _bot.MakeRequestAsync(new SetWebhook($"https://{hostName}/" + webhookPath, new FileToSend(new FileStream(certificate, FileMode.Open), "certificate.pem"))).Result;
                    // todo : log the result
                }
            }
        }
    }
}
