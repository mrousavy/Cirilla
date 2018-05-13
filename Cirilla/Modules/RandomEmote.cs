using System;
using System.Collections.Generic;
using Discord;

namespace Cirilla.Modules
{
    internal class RandomEmote
    {
        internal static IEmote GetRandomEmote(IGuild guild)
        {
            var emotes = new List<GuildEmote>(guild.Emotes);

            //No emojis on that server
            if (emotes.Count < 1)
                return null;

            var random = new Random();
            int emote = random.Next(0, emotes.Count - 1);

            return random.Next(0, Information.RandomReactionChance - 1) == 0 ? emotes[emote] : null;
        }
    }
}