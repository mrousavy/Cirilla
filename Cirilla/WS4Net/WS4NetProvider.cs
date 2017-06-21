using Discord.Net.WebSockets;

// ReSharper disable All

namespace Discord.Net.Providers.WS4Net {
    public static class WS4NetProvider {
        public static readonly WebSocketProvider Instance = () => new WS4NetClient();
    }
}