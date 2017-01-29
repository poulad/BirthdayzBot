# :gift::tada: BirthdayzBot :birthday::beers:

![BirthdayzBot logo](https://cdn.pixabay.com/photo/2013/07/13/13/34/linux-161107_640.png)

## A Telegram chat bot :robot: for the people who forget the birth dates :/
**Add bot to you group chats**. This nice bot is going to remember the birth dates of people in the chat.

> Your birth dates are private! Users have unique birth dates in each chat.

## Requirements
#### .NET Core
This is an **ASP.NET Core 1.1** web application so you need to [install .NET Core](https://www.microsoft.com/net/download/core#/current).

#### API Token
Talk to **[BotFather](t.me/botfather)** to get a token from Telegram for your bot.

> This token is your bot's secret. Keep it safe and never commit it to git.

#### Postgres
This application uses an instance of [PostreSQL](https://www.postgresql.org/) to persist data.
It also is possible to use other databases such as SQLServer but you need to do a few changes to the code.

## Running the bot

#### Configurations
Make a new file in project folder and name it `appsettings.development.json`. Populate it according to this format:
```json
{
  "ApiToken": "",
  "BirthdayzConnectionString": "Host=localhost;Database=birthdayzbot;Username=testybirthdaybot;Password=password",
  "BotName": "Birthdayz_Bot",
  "Certificate": "/path/to/ssl/certificate.pem",
  "HostName": "example.com",
  "UseWebhook": "true"
}
```
Optionally, you could have these configurations as Environment Variables prefixed with `BirthdayzBot_`. Just have a look at the first few lines of `Startup.cs`.

#### Database Migrations
Open a terminal in project's folder and run the following commands:
```bash
~$ dotnet restore
~$ dotnet ef database update
```

Everything is ready now. Run the project and navigate to [http://localhost:5000/botname/apitoken/updates](http://localhost:5000/youbotname/yourapitoken/updates) .
```bash
~$ dotnet run
```
