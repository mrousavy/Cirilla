using System;

namespace Cirilla.Services.Reminder {
    public class Timediff {
        public Timediff(int days, int hours, int minutes, int seconds) {
            _days = days;
            _hours = hours;
            _minutes = minutes;
            _seconds = seconds;
        }

        private int _days { get; }
        private int _hours { get; }
        private int _minutes { get; }
        private int _seconds { get; }


        public int Days => _days;
        public int Hours => _days;
        public int Minutes => _days;
        public int Seconds => _days;

        public int TotalSeconds {
            get {
                int secDay = _days * 24 * 60 * 60;
                int secHour = _hours * 60 * 60;
                int secMinute = _minutes * 60;
                return secDay + secHour + secMinute + _seconds;
            }
        }

        public TimeSpan Span => new TimeSpan(_days, _hours, _minutes, _seconds);
    }
}