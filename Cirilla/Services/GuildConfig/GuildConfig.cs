using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Cirilla.Services.GuildConfig {
    public static class GuildConfigManager {
        public static GuildConfigurations GuildConfigs { get; set; }
        public static List<KeyValuePair<ulong, string>> Prefixes = new List<KeyValuePair<ulong, string>>();


        public static void Init() {
            if (GuildConfigs == null) {
                GuildConfigs = new GuildConfigurations();
            }

            string[] dirs = Directory.GetDirectories(Information.Directory);
            foreach (string dir in dirs) {
                string dirName = Path.GetFileName(Path.GetDirectoryName(dir));
                if (ulong.TryParse(dirName, out ulong guildId)) { // is it a valid Guild _data Directory?
                    string config = Path.Combine(dir, "guildconfig.json");
                    if (!File.Exists(config)) {
                        //create
                        File.Create(config).Dispose();
                        GuildConfiguration guildconfig = new GuildConfiguration {
                            GuildId = guildId
                        };
                        if (!GuildConfigs.GuildConfigs.Contains(guildconfig)) {
                            GuildConfigs.GuildConfigs.Add(guildconfig);
                        } else {
                            GuildConfigs.GuildConfigs.ForEach(gc => {
                                if (gc.GuildId == guildconfig.GuildId) {
                                    gc.Prefix = guildconfig.Prefix;
                                    gc.DisablePrimaryPrefix = guildconfig.DisablePrimaryPrefix;
                                }
                            });
                        }
                        File.WriteAllText(config, JsonConvert.SerializeObject(guildconfig));
                    } else {
                        //load
                        string serialized = File.ReadAllText(config);
                        GuildConfigs.GuildConfigs.Add(JsonConvert.DeserializeObject<GuildConfiguration>(serialized));
                    }
                }
            }
        }


        public static void Set(ulong guildId, string prefix, bool disablePrimaryPrefix) {
            GuildConfiguration guildconfig = new GuildConfiguration {
                GuildId = guildId,
                DisablePrimaryPrefix = disablePrimaryPrefix,
                Prefix = prefix
            };
            if (!GuildConfigs.GuildConfigs.Contains(guildconfig)) {
                GuildConfigs.GuildConfigs.Add(guildconfig);
            } else {
                GuildConfigs.GuildConfigs.ForEach(gc => {
                    if (gc.GuildId == guildconfig.GuildId) {
                        gc.Prefix = guildconfig.Prefix;
                        gc.DisablePrimaryPrefix = guildconfig.DisablePrimaryPrefix;
                    }
                });
            }
            WriteOut();
        }


        public static GuildConfiguration Get(ulong guildId) {
            return GuildConfigs.GuildConfigs.FirstOrDefault(gc => gc.GuildId == guildId);
        }


        public static void WriteOut() {
            string[] dirs = Directory.GetDirectories(Information.Directory);
            foreach (string dir in dirs) {
                string dirName = Path.GetFileName(Path.GetDirectoryName(dir));
                if (ulong.TryParse(dirName, out ulong guildId)) {
                    string config = Path.Combine(dir, "guildconfig.json");
                    if (!File.Exists(config)) {
                        //create
                        File.Create(config).Dispose();
                    }
                    GuildConfiguration guildconfig = GuildConfigs.GuildConfigs.FirstOrDefault(gc => gc.GuildId == guildId);
                    File.WriteAllText(config, JsonConvert.SerializeObject(guildconfig));
                }
            }
        }
    }

    public class GuildConfigurations {
        public List<GuildConfiguration> GuildConfigs { get; set; } = new List<GuildConfiguration>();
    }

    public class GuildConfiguration {
        public string Prefix { get; set; } = Information.SecondaryPrefix;
        public bool DisablePrimaryPrefix { get; set; }
        public ulong GuildId { get; set; }

        protected bool Equals(GuildConfiguration other) {
            return GuildId == other.GuildId;
        }

        public override int GetHashCode() {
            return GuildId.GetHashCode();
        }
    }
}
