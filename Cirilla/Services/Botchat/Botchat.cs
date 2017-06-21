using ChatterBotAPI;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla.Services.Botchat {
    public class Botchat {
        public static ChatterBotSession Bot { get; set; }

        private static int timeout = 1000 * 60; // 1m
        private static CancellationTokenSource Cts { get; set; }

        public static void StartSession() {
            ChatterBotType type = ChatterBotType.PANDORABOTS;
            ChatterBotFactory factory = new ChatterBotFactory();

            ChatterBot chatterBot = factory.Create(type, "b0dafd24ee35a477");
            Bot = chatterBot.CreateSession();
        }


        public static string Send(string message) {
            if (Bot == null) {
                StartSession();
            }
            string answer = Bot.Think(message);

            TriggerCts();

            return answer;
        }

        private static void TriggerCts() {
            Cts?.Cancel();
            Cts = new CancellationTokenSource();
            SessionTimeout();
        }

        public async static void SessionTimeout() {
            try {
                await Task.Delay(timeout, Cts.Token);
                Bot = null;
            } catch (TaskCanceledException) {
                // new message received
            }
        }
    }
}
