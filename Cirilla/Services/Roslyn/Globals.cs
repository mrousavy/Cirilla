using Discord;
using System;
using System.IO;

namespace Cirilla.Services.Roslyn {
    public class Globals {
        public TextWriter Console { get; set; }
        public Random Random { get; set; }
        public IDiscordClient Client { get; set; }
    }
}