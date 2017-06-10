using Discord;
using System;
using System.Collections.Generic;

namespace Cirilla.Modules {
    internal class RandomEmote {

        internal static IEmote GetRandomEmote(IGuild guild, IMessage message) {
            List<GuildEmote> emotes = new List<GuildEmote>(guild.Emotes);
            Random random = new Random();
            int emote = random.Next(0, emotes.Count - 1);

            if (random.Next(0, Information.RandomReactionChance - 1) == 0) {
                return emotes[emote];
            }
            return null;
        }
    }
}
