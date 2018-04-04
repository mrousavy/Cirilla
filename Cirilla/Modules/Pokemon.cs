using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirilla.Services.Pokemon;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class Pokemon : ModuleBase
    {
        [Command("pokedex")]
        [Summary("Search a Pokémon in the national Pokédex by it's ID")]
        public async Task FindPokemon([Summary("The national Pokédex ID of the Pokémon to lookup")]
            int id)
        {
            try
            {
                var message = await ReplyAsync($"Searching Pokémon #{id} in the national Pokédex..");

                var result = await QueryPokedex(id);

                await ReplyAsync("", embed: result);
                await message.DeleteAsync();
            } catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }


        private static async Task<Embed> QueryPokedex(int id)
        {
            var pokemon = await Pokedex.GetPokemonById(id);
            var form = await Pokedex.GetPokemonForm(id);
            string name = pokemon.Name.Capitalize();
            double height = (double) pokemon.Height / 10;
            double weight = (double) pokemon.Weight / 10;

            IList<string> abilities = new List<string>();
            foreach (var ability in pokemon.Abilities.OrderBy(a => a.Slot))
            {
                string appendix = ability.Hidden ? " (Hidden)" : "";
                abilities.Add($"#{ability.Slot}: {ability.Actual.Name.Capitalize()}{appendix}");
            }

            IList<string> items = pokemon.Items.Select(item => item.Name).ToList();

            IList<string> types = pokemon.Types.OrderBy(t => t.Slot)
                .Select(type => $"#{type.Slot}: {type.ToEmoji()} {type.Name}").ToList();

            string joinAbilities = string.Join("\n", abilities);
            joinAbilities = string.IsNullOrWhiteSpace(joinAbilities) ? "/" : joinAbilities;

            string joinItems = string.Join("\n", items);
            joinItems = string.IsNullOrWhiteSpace(joinItems) ? "/" : joinItems;

            string joinTypes = string.Join("\n", types);
            joinTypes = string.IsNullOrWhiteSpace(joinTypes) ? "/" : joinTypes;

            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "National Pokédex",
                    IconUrl = Information.PokedexUrl
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "Powered by pokeapi.co",
                    IconUrl = "http://pokeapi.co"
                },
                Color = new Color(255, 64, 64),
                ThumbnailUrl = form.Sprites.Front
            };
            builder.AddInlineField("Name", $"[{name}](https://bulbapedia.bulbagarden.net/wiki/{name})");
            builder.AddInlineField("ID", $"#{pokemon.Id}");
            builder.AddInlineField("Height", $"{height} m");
            builder.AddInlineField("Weight", $"{weight} kg");
            builder.AddInlineField("Base Exp.", pokemon.BaseExperience);
            builder.AddInlineField("Abilities", joinAbilities);
            builder.AddInlineField("Default", pokemon.Default ? "Yes" : "No");
            builder.AddInlineField("Order", pokemon.Order);
            builder.AddInlineField("Held Items", joinItems);
            builder.AddInlineField("Type", joinTypes);
            return builder.Build();
        }
    }
}