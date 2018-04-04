using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cirilla.Services.Pokemon
{
    public class Pokemon
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = "Missigno";

        [JsonProperty(PropertyName = "base_experience")]
        public int BaseExperience { get; set; }

        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "is_default")]
        public bool Default { get; set; } = true;

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; } = 1;

        [JsonProperty(PropertyName = "weight")]
        public int Weight { get; set; }

        [JsonProperty(PropertyName = "abilities")]
        public IEnumerable<Ability> Abilities { get; set; }

        [JsonProperty(PropertyName = "forms")]
        public IEnumerable<Property> Forms { get; set; }

        [JsonProperty(PropertyName = "game_indices")]
        public IEnumerable<GameIndex> Indices { get; set; }

        [JsonProperty(PropertyName = "held_items")]
        public IEnumerable<Property> Items { get; set; }

        [JsonProperty(PropertyName = "location_area_encounters")]
        public string Encounters { get; set; }

        [JsonProperty(PropertyName = "moves")]
        public IEnumerable<Move> Moves { get; set; }

        [JsonProperty(PropertyName = "species")]
        public Property Species { get; set; }

        [JsonProperty(PropertyName = "stats")]
        public IEnumerable<Statistic> Stats { get; set; }

        [JsonProperty(PropertyName = "types")]
        public IEnumerable<Type> Types { get; set; }
    }

    public class Ability
    {
        [JsonProperty(PropertyName = "is_hidden")]
        public bool Hidden { get; set; } = true;

        [JsonProperty(PropertyName = "slot")]
        public int Slot { get; set; }

        [JsonProperty(PropertyName = "ability")]
        public Property Actual { get; set; }
    }

    /// <summary>
    ///     Indicating any Object in the Pokedex API that has a name and a URL
    /// </summary>
    public class Property
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class GameIndex
    {
        [JsonProperty(PropertyName = "game_index")]
        public int Index { get; set; }

        public string Name => Version.Name;
        public string Url => Version.Url;

        [JsonProperty(PropertyName = "version")]
        public Property Version { get; set; }
    }

    public class GroupDetails
    {
        [JsonProperty(PropertyName = "level_learned_at")]
        public int LearnedAt { get; set; }

        [JsonProperty(PropertyName = "version_group")]
        public Property Group { get; set; }

        [JsonProperty(PropertyName = "move_learn_method")]
        public Property LearnMethod { get; set; }
    }

    public class Move
    {
        [JsonProperty(PropertyName = "move")]
        public Property Actual { get; set; }

        [JsonProperty(PropertyName = "version_group_details")]
        public IEnumerable<GroupDetails> GroupDetails { get; set; }
    }

    public class Statistic
    {
        [JsonProperty(PropertyName = "base_stat")]
        public int BaseValue { get; set; }

        [JsonProperty(PropertyName = "effort")]
        public int EffortValue { get; set; }

        public string Name => Actual.Name;
        public string Url => Actual.Url;

        [JsonProperty(PropertyName = "stat")]
        public Property Actual { get; set; }
    }

    public class Type
    {
        [JsonProperty(PropertyName = "slot")]
        public int Slot { get; set; }

        public string Name => Actual.Name;
        public string Url => Actual.Url;

        [JsonProperty(PropertyName = "type")]
        public Property Actual { get; set; }

        public string ToEmoji()
        {
            switch (Name)
            {
                case "normal":
                    return "✊";
                case "fire":
                    return "🔥";
                case "fighting":
                    return "🥊";
                case "water":
                    return "🌊";
                case "flying":
                    return "🐦";
                case "grass":
                    return "🌿";
                case "poison":
                    return "🍼";
                case "electric":
                    return "⚡";
                case "ground":
                    return "🕳️";
                case "psychic":
                    return "🧙";
                case "rock":
                    return "🧗‍♂️";
                case "ice":
                    return "🍦";
                case "bug":
                    return "🐛";
                case "dragon":
                    return "🐉";
                case "ghost":
                    return "👻";
                case "dark":
                    return "🧛🏿";
                case "steel":
                    return "🔩";
                case "fairy":
                    return "🧚";
                default:
                    return "❓";
            }
        }
    }


    public class Form
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "form_name")]
        public string FormName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = "Missigno";

        [JsonProperty(PropertyName = "is_default")]
        public bool Default { get; set; }

        [JsonProperty(PropertyName = "is_battle_only")]
        public bool BattleOnly { get; set; }

        [JsonProperty(PropertyName = "sprites")]
        public Sprites Sprites { get; set; }

        [JsonProperty(PropertyName = "version_group")]
        public Property VersionGroup { get; set; }

        [JsonProperty(PropertyName = "form_order")]
        public int FormOrder { get; set; }

        [JsonProperty(PropertyName = "is_mega")]
        public bool IsMega { get; set; }

        [JsonProperty(PropertyName = "form_names")]
        public IEnumerable<string> FormNames { get; set; }

        [JsonProperty(PropertyName = "names")]
        public IEnumerable<string> Names { get; set; }

        [JsonProperty(PropertyName = "pokemon")]
        public Property Pokemon { get; set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }
    }

    public class Sprites
    {
        [JsonProperty(PropertyName = "front_default")]
        public string Front { get; set; }

        [JsonProperty(PropertyName = "back_default")]
        public string Back { get; set; }

        [JsonProperty(PropertyName = "front_shiny")]
        public string FrontShiny { get; set; }

        [JsonProperty(PropertyName = "back_shiny")]
        public string BackShiny { get; set; }
    }
}