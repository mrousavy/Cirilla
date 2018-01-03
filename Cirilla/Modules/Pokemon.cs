using System;
using System.Threading.Tasks;
using Cirilla.Services.Pokemon;
using Discord.Commands;

namespace Cirilla.Modules {
    public class Pokemon : ModuleBase {
        [Command("pokedex")]
        [Summary("Search a Pokémon in the national Pokédex by it's ID")]
        public async Task FindPokemon([Summary("The national Pokédex ID of the Pokémon to lookup")] int id) {
            try {
                var pokemon = await Pokedex.GetPokemonById(id);
                // TODO: Build Pokemon Embed
            } catch (Exception ex) {
                await ReplyAsync(ex.Message);
            }
        }
    }
}