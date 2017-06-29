<p align="center">
  <img src="https://raw.githubusercontent.com/mrousavy/Cirilla/master/Resources/Ciri_round.png" height="120" />
  <h3 align="center">Cirilla</h3>
  <p align="center">The swiss army knife discord bot</p>
  <p align="center">
    <a href="https://discordapp.com/oauth2/authorize?client_id=323123443136593920&scope=bot&permissions=67184707"><img src="https://img.shields.io/badge/Add%20to%20your-Discord-9399ff.svg" alt="Add to your discord"></a>
    <a href="https://discord.gg/ebXZnFX"><img src="https://discordapp.com/api/guilds/326668996550197249/widget.png" alt="Test it"></a>
  </p>
</p>

# Features
Cirilla is a multi-use Discord Bot with many functionalities like:
* A whole **XP System** _(disabled by default)_
* Random posts from **Reddit** or **dankmemes**
* Custom **Reminders**
* **Poking** users
* Setting up custom **Hardware** or **Profile** pages
* Adding important **links** to your guild
* Calculating math expressions
* **News**
* Querying **wikipedia**
* Defining some words
* Votekicking users _(disabled by default)_
* **C# Code/Script** compiling and executing _(disabled by default)_
* **Chat cleaning**

# Setup
1. [Add Cirilla to your guild](https://discordapp.com/oauth2/authorize?client_id=323123443136593920&scope=bot&permissions=67184707) (for 100% functionality you want to keep all the checkboxes checked on the auth-page)
2. Change the prefix you want to use (e.g.: `$prefix ?` will change the prefix to `?`)
3. If you don't want the primary/default prefix (`$`) at all, disable it with: `$toggleprimary`
4. View the current config (`$config` or `$host`) to be sure it's setup correctly
5. XP Files can be added manually when you run `$xp` (for yourself) or `$xp USERNAME` (for a specific user) once.
6. Try out some [Commands](#Commands)!

# Commands
These are the commands you can use:

**Admin**
```
$nick nickname
$setup 
$leave 
$prefix prefix
$toggleprimary
$togglexp
```
**BotInfo**
```
$host 
$invite 
$uptime 
$run 
$source
$owner
```
**Chat**
```
$embed text
$hi 
$hello
$poke user
$poke user message
$say message
```
**Clean**
```
$clean 
$clean count
```
**Code**
```
$exec code
```
**Config**
```
$config 
$reload
```
**Connection**
```
$ping
```
**GitHub**
```
$bugreport 
$repo 
$addmodule
```
**Hardware**
```
$hw user
$hw 
$sethw title
```
**Help**
```
$help 
$help command
```
**Link**
```
$links 
$addlink name, link
$removelink name
```
**Maths**
```
$square num
$sqrt num
$calc expression
$pi
```
**News**
```
$news 
$news limit
```
**Owner**
```
$announce text
$cmdlog
$log 
$clearlog 
$game game
$reboot 
$shutdown
$togglescripts
```
**Profile**
```
$profile user
$profile 
$setprofile
```
**Randoms**
```
$flip 
$random maximum
```
**Reddit**
```
$dankmeme 
$reddit rsubreddit
```
**Reminder**
```
$remindme time, text
```
**Search**
```
$define words
$google query
$wiki query
```
**UserInfo**
```
$avatar
$avatar user
$details
$stats user
$stats
```
**Votekick**
```
$votekick user
```
**Xp**
```
$xp user, xp
$setxp user, xp
$xp 
$xp user
$stats
```
