using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using Newtonsoft.Json;

namespace Cirilla.Services.Profiles
{
    public static class ProfileManager
    {
        public static UserProfiles Profiles;

        public static string ReadProfile(IGuild guild, ulong userId)
        {
            string profiles = Helper.GetPath(guild, "profiles.json");
            if (File.Exists(profiles))
            {
                Profiles = JsonConvert.DeserializeObject<UserProfiles>(File.ReadAllText(profiles));

                var profile = Profiles.ProfilesList.FirstOrDefault(p => p.UserId == userId);
                return profile?.ProfileText;
            }

            return null;
        }

        public static void UpdateProfile(IGuild guild, ulong userId, string text)
        {
            string profiles = Helper.GetPath(guild, "profiles.json");

            if (Profiles == null)
                Profiles = new UserProfiles();

            bool contains = false;

            foreach (var uprof in Profiles.ProfilesList)
                if (uprof.UserId == userId)
                {
                    uprof.ProfileText = text;
                    contains = true;
                    break;
                }

            if (!contains) Profiles.ProfilesList.Add(new UserProfile(userId, text));

            File.WriteAllText(profiles, JsonConvert.SerializeObject(Profiles));
        }

        public class UserProfiles
        {
            public UserProfiles()
            {
                ProfilesList = new List<UserProfile>();
            }

            public List<UserProfile> ProfilesList { get; set; }
        }

        public class UserProfile
        {
            public UserProfile(ulong id, string text)
            {
                UserId = id;
                ProfileText = text ?? string.Empty;
            }

            public ulong UserId { get; set; }
            public string ProfileText { get; set; }
        }
    }
}