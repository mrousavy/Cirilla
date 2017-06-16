using System.IO;
using Discord;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cirilla.Services.Permissions {
    public static class PermissionValidator {
        public static async void ValidatePermission(IGuild guild) {
            ulong id = guild.Id;
            if (IsAlreadyValidated(id)) {
                return;
            }

            IGuildUser user = await guild.GetCurrentUserAsync();
            GuildPermissions permissions = user.GuildPermissions;

            if (permissions.AddReactions &&
                permissions.CreateInstantInvite &&
                permissions.KickMembers &&
                permissions.ManageMessages &&
                permissions.ReadMessageHistory && //needed?
                permissions.ReadMessages && //needed?
                permissions.SendMessages
            ) {
                // has permissions
                SetValidated(id);
                await ConsoleHelper.Log($"\"{guild.Name}\" successfully validated!", LogSeverity.Info);
            } else {
                await guild.LeaveAsync();
                await ConsoleHelper.Log($"\"{guild.Name}\" does not grant required permissions! Server left!", LogSeverity.Info);
            }
        }

        //Is that Guild already validated by ValidatePermission(IGuild)
        public static bool IsAlreadyValidated(ulong id) {
            string guildsFile = Path.Combine(Information.Directory, "guilds.json");

            if (!File.Exists(guildsFile)) {
                string serialized = JsonConvert.SerializeObject(new GuildsFile());
                File.WriteAllText(guildsFile, serialized);
                return false;
            } else {
                string serialized = File.ReadAllText(guildsFile);
                GuildsFile file = JsonConvert.DeserializeObject<GuildsFile>(serialized);
                if (file.GuildIds.Contains(id)) {
                    return true;
                }
            }
            return false;
        }

        //Add the Id to the validated-guilds file
        public static void SetValidated(ulong id) {
            string guildsFile = Path.Combine(Information.Directory, "guilds.json");

            if (!File.Exists(guildsFile)) {
                GuildsFile file = new GuildsFile();
                file.GuildIds.Add(id);
                string serialized = JsonConvert.SerializeObject(file);
                File.WriteAllText(guildsFile, serialized);
            } else {
                string serialized = File.ReadAllText(guildsFile);
                GuildsFile file = JsonConvert.DeserializeObject<GuildsFile>(serialized);
                file.GuildIds.Add(id);
                serialized = JsonConvert.SerializeObject(file);
                File.WriteAllText(guildsFile, serialized);
            }
        }


        public class GuildsFile {
            public List<ulong> GuildIds { get; set; } = new List<ulong>();
        }
    }
}
