using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;

namespace Cirilla.Services.Reminder
{
    public class ReminderTypeReader : TypeReader
    {
        private const string _tdn = "([0-9]|0[0-9]|1[0-9]|2[0-3])"; // Two Digit Number

        private static readonly Regex _day = new Regex($"{_tdn}d", RegexOptions.IgnoreCase);
        private static readonly Regex _hour = new Regex($"{_tdn}h", RegexOptions.IgnoreCase);
        private static readonly Regex _minute = new Regex($"{_tdn}m", RegexOptions.IgnoreCase);
        private static readonly Regex _second = new Regex($"{_tdn}s", RegexOptions.IgnoreCase);

        public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            try
            {
                var split = input.Split(':');

                var dayMatch = _day.Match(input);
                var hourMatch = _hour.Match(input);
                var minuteMatch = _minute.Match(input);
                var secondMatch = _second.Match(input);

                string dayValue = string.IsNullOrWhiteSpace(dayMatch.Value) ? "0" : dayMatch.Value.Replace("d", "");
                string hourValue = string.IsNullOrWhiteSpace(hourMatch.Value) ? "0" : hourMatch.Value.Replace("h", "");
                string minuteValue = string.IsNullOrWhiteSpace(minuteMatch.Value)
                    ? "0"
                    : minuteMatch.Value.Replace("m", "");
                string secondValue = string.IsNullOrWhiteSpace(secondMatch.Value)
                    ? "0"
                    : secondMatch.Value.Replace("s", "");

                int days = int.Parse(dayValue);
                int hours = int.Parse(hourValue);
                int minutes = int.Parse(minuteValue);
                int seconds = int.Parse(secondValue);

                var diff = new Timediff(days, hours, minutes, seconds);
                if (diff.TotalSeconds < 1)
                    throw new Exception();

                return Task.FromResult(TypeReaderResult.FromSuccess(diff));
            } catch
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid format"));
            }
        }
    }
}