using Discord.WebSocket;
using System;
using System.IO;

namespace Cirilla.Services.Roslyn {
    public class Globals {
        public TextWriter Console { get; set; }
        public Random Random { get; set; }
        public DiscordSocketClient Client { get; set; }
        public Action<string> ReplyAsync { get; set; }
    }
}