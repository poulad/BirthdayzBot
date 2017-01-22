# :gift::tada: BirthdayzBot :birthday::beers:

![BirthdayzBot logo](https://cdn.pixabay.com/photo/2013/07/13/13/34/linux-161107_640.png)

## A Telegram chat bot :robot: for the people who forget the birth dates :/
**Add bot to you group chats**. This nice bot is going to remember the birth dates of people in the chat.



> Your birth dates are private! Users have unique birth dates in each chat.


### Configurations
There are 3 configurations that bot needs to use: 

- `AccessToken`: Telegram's API token
- `BirthdayzConnectionString`: Database connection
- `BotName`: Name of the bot in Telegram.

and you can set them up in 2 ways: Environment Variables or `Config.json` file.

#### Environment Variables:
Prefix your variable names with `Birthdayz_` and you are good to go.

For example:
`export Birthdayz_BotName="Birthdayz_Bot"`

#### Config.json file:
You can safely put the configs in that file and it is gitignored. Use the example file.

`
cp config.example.json
`
