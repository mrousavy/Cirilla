using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cirilla.Services.Pokemon
{
    public static class Pokedex
    {
        public static async Task<Pokemon> GetPokemonById(int id)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"http://pokeapi.co/api/v2/pokemon/{id}", null);
                string content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    throw new Exception("The server returned no results!");
                if (content.ToLower().Contains("not found"))
                    throw new Exception($"Pokémon with ID #{id} not found!");
                return JsonConvert.DeserializeObject<Pokemon>(content);
            }
        }

        public static async Task<Form> GetPokemonForm(int id)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"http://pokeapi.co/api/v2/pokemon-form/{id}", null);
                string content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    throw new Exception("The server returned no results!");
                if (content.ToLower().Contains("not found"))
                    throw new Exception($"Pokémon Form with ID #{id} not found!");
                return JsonConvert.DeserializeObject<Form>(content);
            }
        }
    }
}