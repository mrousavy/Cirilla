using System;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace Cirilla.Services.Roslyn {
    public class Globals {
        public TextWriter Console { get; set; }
        public Random Random { get; set; }
        public Func<string, Task<IUserMessage>> ReplyAsync { get; set; }
    }
}