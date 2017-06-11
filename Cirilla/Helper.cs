using Discord;

namespace Cirilla {
    public static class Helper {
        public static object Lock { get; set; } = new object();

        public static string GetName(IUser user) {
            if (user == null) {
                return "[Unknown]";
            }

            IGuildUser guildUser = user as IGuildUser;
            if (guildUser == null) {
                return string.IsNullOrWhiteSpace(user.Username) ? user.ToString() : user.Username;
            } else {
                return string.IsNullOrWhiteSpace(guildUser.Nickname) ? user.Username : guildUser.Nickname;
            }
        }
    }
}
