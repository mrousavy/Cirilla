using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using Newtonsoft.Json;

namespace Cirilla.Services.GuildConfig {
    public static class GuildConfigManager {
        public static List<KeyValuePair<ulong, string>> Prefixes = new List<KeyValuePair<ulong, string>>();
        public static GuildConfigurations GuildConfigs { get; set; }


        public static void Init() {
            if (GuildConfigs == null) GuildConfigs = new GuildConfigurations();

            string[] dirs = Directory.GetDirectories(Information.Directory);
            ConsoleHelper.Log($"Loading {dirs.Length} Guild configuration files..", LogSeverity.Info);
            int loaded = 0;

            foreach (string dir in dirs) {
                string dirName = Path.GetFileName(dir);
                if (ulong.TryParse(dirName, out ulong guildId)) {
                    // is it a valid Guild _data Directory?
                    string config = Path.Combine(dir, "guildconfig.json");
                    if (!File.Exists(config)) {
                        //create
                        File.Create(config).Dispose();
                        var guildconfig = new GuildConfiguration {
                            GuildId = guildId
                        };
                        if (!GuildConfigs.GuildConfigs.Contains(guildconfig))
                            GuildConfigs.GuildConfigs.Add(guildconfig);
                        else
                            GuildConfigs.GuildConfigs.ForEach(gc => {
                                if (gc.GuildId == guildconfig.GuildId) {
                                    gc.Prefix = guildconfig.Prefix;
                                    gc.EnablePrimaryPrefix = guildconfig.EnablePrimaryPrefix;
                                }
                            });
                        File.WriteAllText(config, JsonConvert.SerializeObject(guildconfig));
                    } else {
                        //load
                        string serialized = File.ReadAllText(config);
                        GuildConfigs.GuildConfigs.Add(JsonConvert.DeserializeObject<GuildConfiguration>(serialized));
                        loaded++;
                    }
                }
            }
            ConsoleHelper.Log($"{loaded}/{dirs.Length} guild configuration files loaded!", LogSeverity.Info);
        }


        public static GuildConfiguration Set(ulong guildId, string prefix, bool enablePrimaryPrefix = true,
            bool enableXpSystem = false) {
            var guildconfig = new GuildConfiguration {
                GuildId = guildId,
                EnablePrimaryPrefix = enablePrimaryPrefix,
                Prefix = prefix,
                EnableXpSystem = enableXpSystem
            };
            bool contains = GuildConfigs.GuildConfigs.Any(gc => gc.GuildId == guildId);
            if (contains)
                GuildConfigs.GuildConfigs.ForEach(gc => {
                    if (gc.GuildId == guildconfig.GuildId) {
                        gc.Prefix = guildconfig.Prefix;
                        gc.EnablePrimaryPrefix = guildconfig.EnablePrimaryPrefix;
                        gc.EnableXpSystem = guildconfig.EnableXpSystem;
                    }
                });
            else GuildConfigs.GuildConfigs.Add(guildconfig);
            WriteOut();
            return guildconfig;
        }

        public static GuildConfiguration Set(GuildConfiguration guildconfig) {
            bool contains = GuildConfigs.GuildConfigs.Any(gc => gc.GuildId == guildconfig.GuildId);
            if (contains)
                GuildConfigs.GuildConfigs.ForEach(gc => {
                    if (gc.GuildId == guildconfig.GuildId) {
                        gc.Prefix = guildconfig.Prefix;
                        gc.EnablePrimaryPrefix = guildconfig.EnablePrimaryPrefix;
                        gc.EnableXpSystem = guildconfig.EnableXpSystem;
                    }
                });
            else GuildConfigs.GuildConfigs.Add(guildconfig);
            WriteOut();
            return guildconfig;
        }


        public static GuildConfiguration Get(ulong guildId) {
            var config = GuildConfigs.GuildConfigs.FirstOrDefault(gc => gc.GuildId == guildId);
            if (config == default(GuildConfiguration))
                return new GuildConfiguration {
                    GuildId = guildId
                };
            return config;
        }


        public static void WriteOut() {
            string[] dirs = Directory.GetDirectories(Information.Directory);
            foreach (string dir in dirs) {
                string dirName = Path.GetFileName(dir);
                if (ulong.TryParse(dirName, out ulong guildId)) {
                    string config = Path.Combine(dir, "guildconfig.json");
                    if (!File.Exists(config)) File.Create(config).Dispose();
                    var guildconfig =
                        GuildConfigs.GuildConfigs.FirstOrDefault(gc => gc.GuildId == guildId);
                    if (guildconfig == default(GuildConfiguration))
                        guildconfig = new GuildConfiguration {
                            EnablePrimaryPrefix = true,
                            GuildId = guildId,
                            Prefix = Information.SecondaryPrefix
                        };
                    File.WriteAllText(config, JsonConvert.SerializeObject(guildconfig));
                }
            }
        }
    }

    public class GuildConfigurations {
        public List<GuildConfiguration> GuildConfigs { get; set; } = new List<GuildConfiguration>();
    }

    public class GuildConfiguration {
        public ulong GuildId { get; set; }

        public string Prefix { get; set; } = Information.SecondaryPrefix;
        public bool EnablePrimaryPrefix { get; set; } = true;
        public bool EnableXpSystem { get; set; }
        public bool EnableScripts { get; set; } //Get only, this will only be set by me in config

        protected bool Equals(GuildConfiguration other) {
            return GuildId == other.GuildId;
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return GuildId.GetHashCode();
        }
    }
}