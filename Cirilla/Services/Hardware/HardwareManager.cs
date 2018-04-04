using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using Newtonsoft.Json;

namespace Cirilla.Services.Hardware
{
    public class HardwareManager
    {
        public static UserHardwareFile UserHardwares;

        public static Tuple<string, string> ReadHardware(IGuild guild, ulong userId)
        {
            string hwfile = Helper.GetPath(guild, "hardware.json");
            if (File.Exists(hwfile))
            {
                UserHardwares = JsonConvert.DeserializeObject<UserHardwareFile>(File.ReadAllText(hwfile));

                var hw = UserHardwares.Hardwares.FirstOrDefault(h => h.UserId == userId);
                return hw == null ? null : new Tuple<string, string>(hw.Title, hw.Hardware);
            }

            return null;
        }

        public static void UpdateHardware(IGuild guild, ulong userId, string title, string hardware)
        {
            string hwfile = Helper.GetPath(guild, "hardware.json");

            if (UserHardwares == null)
                UserHardwares = new UserHardwareFile();

            bool contains = false;

            foreach (var usrhw in UserHardwares.Hardwares)
                if (usrhw.UserId == userId)
                {
                    if (!string.IsNullOrWhiteSpace(title))
                        usrhw.Title = title;
                    if (!string.IsNullOrWhiteSpace(hardware))
                        usrhw.Hardware = hardware;
                    contains = true;
                    break;
                }

            if (!contains) UserHardwares.Hardwares.Add(new UserHardware(userId, title, hardware));

            File.WriteAllText(hwfile, JsonConvert.SerializeObject(UserHardwares));
        }

        public class UserHardwareFile
        {
            public UserHardwareFile()
            {
                Hardwares = new List<UserHardware>();
            }

            public List<UserHardware> Hardwares { get; set; }
        }

        public class UserHardware
        {
            public UserHardware(ulong id, string title, string hardware)
            {
                UserId = id;
                Title = title ?? string.Empty;
                Hardware = hardware ?? string.Empty;
            }

            public ulong UserId { get; set; }
            public string Hardware { get; set; }
            public string Title { get; set; }
        }
    }
}