using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cirilla.Services.Hardware {
    public class HardwareManager {
        public static Tuple<string, string> ReadHardware(ulong userId) {
            string hwfile = Path.Combine(Information.Directory, "hardware.json");
            if (File.Exists(hwfile)) {
                UserHardwares = JsonConvert.DeserializeObject<UserHardwareFile>(File.ReadAllText(hwfile));

                UserHardware hw = UserHardwares.Hardwares.FirstOrDefault(h => h.UserId == userId);
                if (hw == null) {
                    return null;
                }
                return new Tuple<string, string>(hw.Title, hw.Hardware);
            } else {
                return null;
            }
        }

        public static void UpdateHardware(ulong userId, string title, string hardware) {
            string hwfile = Path.Combine(Information.Directory, "hardware.json");

            if (UserHardwares == null)
                UserHardwares = new UserHardwareFile();

            bool contains = false;

            foreach (UserHardware usrhw in UserHardwares.Hardwares) {
                if (usrhw.UserId == userId) {
                    if (!string.IsNullOrWhiteSpace(title))
                        usrhw.Title = title;
                    if (!string.IsNullOrWhiteSpace(hardware))
                        usrhw.Hardware = hardware;
                    contains = true;
                    break;
                }
            }

            if (!contains) {
                UserHardwares.Hardwares.Add(new UserHardware(userId, title, hardware));
            }

            File.WriteAllText(hwfile, JsonConvert.SerializeObject(UserHardwares));
        }


        public static UserHardwareFile UserHardwares;

        public class UserHardwareFile {
            public UserHardwareFile() { Hardwares = new List<UserHardware>(); }

            public List<UserHardware> Hardwares { get; set; }
        }

        public class UserHardware {
            public UserHardware(ulong id, string title, string hardware) {
                UserId = id;
                Title = title;
                Hardware = hardware;
            }

            public ulong UserId { get; set; }
            public string Hardware { get; set; }
            public string Title { get; set; }
        }
    }
}