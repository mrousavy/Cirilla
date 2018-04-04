using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Cirilla.Services.Hardware
{
    public class HardwareListener
    {
        private readonly ITextChannel _channel;
        private readonly IUser _user;
        private bool _done;

        public HardwareListener(ITextChannel channel, IUser user)
        {
            _channel = channel;
            _user = user;
            Hourglass();
        }

        public async Task HardwareReceived(SocketMessage arg)
        {
            try
            {
                var message = arg as SocketUserMessage;
                if (message == null)
                    return;

                IUser user = message.Author;
                if (message.Channel is ITextChannel channel && user.Id == _user.Id && channel.Id == _channel.Id)
                {
                    Cirilla.Client.MessageReceived -= HardwareReceived;

                    HardwareManager.UpdateHardware(channel.Guild, user.Id, null, message.Content);
                    _done = true;
                    ConsoleHelper.Log($"Created hardware profile for {_user}!", LogSeverity.Info);
                    await arg.Channel.SendMessageAsync("Hardware successfully set!");
                }
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not create hardware profile for {_user}! ({ex.Message})",
                    LogSeverity.Error);
                await arg.Channel.SendMessageAsync("Sorry, I couldn't set your Hardware :confused:");
            }
        }

        public async void Hourglass()
        {
            //time in Minutes until the bot stops listening
            int time = 5;
            await Task.Delay(1000 * 60 * time);
            if (!_done)
            {
                try
                {
                    Cirilla.Client.MessageReceived -= HardwareReceived;
                } catch
                {
                    // event already removed
                }

                await _channel.SendMessageAsync($"Sorry {Helper.GetName(_user)}, I'm out of time! :hourglass:");
            }
        }
    }
}