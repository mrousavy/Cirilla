﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Cirilla.Services.Profiles
{
    public class ProfileListener
    {
        private readonly ITextChannel _channel;
        private readonly IUser _user;
        private bool _done;

        public ProfileListener(ITextChannel channel, IUser user)
        {
            _channel = channel;
            _user = user;
            Hourglass();
        }

        public async Task ProfileTextReceived(SocketMessage arg)
        {
            try
            {
                var message = arg as SocketUserMessage;
                if (message == null)
                    return;

                IUser user = message.Author;
                if (message.Channel is ITextChannel channel && user.Id == _user.Id && channel.Id == _channel.Id)
                {
                    Cirilla.Client.MessageReceived -= ProfileTextReceived;

                    ProfileManager.UpdateProfile(channel.Guild, user.Id, message.Content);
                    _done = true;
                    await arg.Channel.SendMessageAsync("Profile set successfully!");
                    ConsoleHelper.Log($"{Helper.GetName(_user)} created a new profile!", LogSeverity.Info);
                }
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not create profile! ({ex.Message})", LogSeverity.Error);
                await arg.Channel.SendMessageAsync("Sorry, I couldn't set your Profile Information :confused:");
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
                    Cirilla.Client.MessageReceived -= ProfileTextReceived;
                } catch
                {
                    // event already removed
                }

                await _channel.SendMessageAsync("Sorry, I'm out of time! :hourglass:");
            }
        }
    }
}