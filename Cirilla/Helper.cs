using Discord;

namespace Cirilla {
    public static class Helper {
        public static object Lock { get; set; } = new object();

        public static string GetName(IUser user) {
            IGuildUser guildUser = user as IGuildUser;
            if (guildUser == null)
                return user.Username;
            else
                return guildUser.Nickname;
        }
    }
}
