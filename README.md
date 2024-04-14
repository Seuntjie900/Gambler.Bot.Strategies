This project contains the code I've written so far for the automated betting and strategy library of what I envisioned to be DiceBot v4. 

# How to use
coming soon.




A summary of stuff that doormat.bot should be capable of when finished

- .net standard library to allow cross platform betting.
- Multi game support. The strategy and site interfaces (and programmer modes) were designed to handle multiple games, not just dice anymore.
- light distribution of updates. I had to remove some of the code handling it after moving to .net standard and I haven't fixed everything again yet, but the idea is that you can compile files that implement the site interface and the strategy interface when starting up the bot (Or while it's running), allowing one to push updates and fixes for sites and strategies without having to update the whole bot.
- Multiple programmer modes. This version already supports LUA, JS and C#. I had python support, but the ironpython libraries are not yet .net standard compatible. (They are .net core compatible though)
- More info in the programmer mode. You now have readonly access to all of the stats that DiceBot tracks internally from within the programmer mode as well as the sitestats object for your account at the site.
- Error handling within the programmer mode. For errors raised, an event will fire in the programmer mode (if your script handles it) and you can decide how to handle the error. If it is not handled, it's deferred to the settings mentioned below.
- Better error handling. You now have options for how the bot handles certain errors. You can specify the bot to continue/retry/stop on a bet error or withdrawal error etc.
- Notifications triggers. You can set up a notifications to be sent based on criteria you can specify, using any stat tracked by the bot internally. (for example if losingstreak>20 send an email and show a desktop notification. If profit is > 10% of balance, play a sound). The wrapping application needs to handle the notifications
- Actions triggers, Like notifications, you can set up trigger actions based on any stat in the stats object and give instructions to the bot to perform an action, like stopping, withdrawing, tipping etc.
- n tier database support. When starting up the bot, you have the option to specify which database type you want to connect to and specify connection strings etc. Currently implement is sqlite, mssql, mysql, postgre. I planned to integrate mongo as well, but never got around to it. I've lightly tested sqlite, mssql and mysql and they seemed to be working fairly well. Never got around to testing postgre.
- Faster simulations. Like waaaay faster. Like 10 000 000 bet simulation in 7 minutes fast.
- Faster bets. Removed a lot of overhead of waiting for GUI updates etc, so bets might be noticibly faster for sites like yolodice, but mostly the bot will just be more resource efficient. Depends on how the UI is implemented of course.
- More user settings. Like the error handling, notifications and action triggers, give more control to the user over everything in the bot.
- KeePass 2 integration. The keepass project is pulled from https://github.com/Strangelovian/KeePass2Core. I couldn't get it to work properly just referencing the dll, so I just pulled in the project. (I spent a whole 5 minutes of trying though. I'm sure someone can do better)

The api for the programmer modes have changed. Instead of having a single dobet function and a bunch of global variables, you have a reset function that is required, with a bet parameter (the name and type of bet object depends on the game you're running). The values you set for this bet parameter is your initial bet and will be the bet used when a reset is triggered in the bot. You then also have a dobet function with 3 parameters, your previous bet object, a boolean to specify whether the bet was a win or not, and a next bet object. The name of the method and types of objects for previous bet and next bet depends on the game you're playing. The values set in the nextbet obect will be used to place the next bet. You additionally have some global variables that are updated after every bet (and are read-only) that contains stats, site info and stats received from the site. (these are objects, not just variables anymore).

There's a programmer mode doc and a readme file that provides more information on how the triggers and settings work, and the new programmer mode API. There's also some sample scripts for each of the programmer mod that might or might not work.