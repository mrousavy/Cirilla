using Discord;
using System;
using System.IO;
using Discord.WebSocket;

namespace Cirilla.Services.Roslyn {
    public class Globals {
        public TextWriter Console { get; set; }
        public Random Random { get; set; }
        public DiscordSocketClient Client { get; set; }
    }
}