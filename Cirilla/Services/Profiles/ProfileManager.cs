using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cirilla.Services.Profiles {
    public static class ProfileManager {
        public static string ReadProfile(ulong userId) {
            string profiles = Path.Combine(Information.Directory, "profiles.json");
            if (File.Exists(profiles)) {
                Profiles = JsonConvert.DeserializeObject<UserProfiles>(File.ReadAllText(profiles));

                UserProfile profile = Profiles.ProfilesList.FirstOrDefault(p => p.UserId == userId);
                return profile?.ProfileText;
            } else {
                return null;
            }
        }

        public static void UpdateProfile(ulong userId, string text) {
            string profiles = Path.Combine(Information.Directory, "profiles.json");

            if (Profiles == null)
                Profiles = new UserProfiles();

            bool contains = false;

            foreach (UserProfile uprof in Profiles.ProfilesList) {
                if (uprof.UserId == userId) {
                    uprof.ProfileText = text;
                    contains = true;
                    break;
                }
            }

            if (!contains) {
                Profiles.ProfilesList.Add(new UserProfile(userId, text));
            }

            File.WriteAllText(profiles, JsonConvert.SerializeObject(Profiles));
        }


        public static UserProfiles Profiles;

        public class UserProfiles {
            public UserProfiles() {
                ProfilesList = new List<UserProfile>();
            }

            public List<UserProfile> ProfilesList { get; set; }
        }

        public class UserProfile {
            public UserProfile(ulong id, string text) {
                UserId = id;
                ProfileText = text;
            }

            public ulong UserId { get; set; }
            public string ProfileText { get; set; }
        }
    }
}
