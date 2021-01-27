using System;
namespace WeatherDataLibrary
{
    public class BalconyOpen
    {
        public BalconyOpen(DateTime date, TimeSpan timeOpen, int timesOpened)
        {
            Date = date;
            TimeOpen = timeOpen;
            TimesOpened = timesOpened;
        }

        public DateTime Date { get; set; }
        public TimeSpan TimeOpen { get; set; }
        public int TimesOpened { get; set; }
    }
}
