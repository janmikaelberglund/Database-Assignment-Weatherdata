using System;
namespace WeatherDataLibrary
{
    public class DatabaseRecord
    {
        public DateTime Date { get; set; }
        public double? Measurement { get; set; }
        public string Room { get; set; }

        public DatabaseRecord(DateTime date, double? measurement)
        {
            Date = date;
            Measurement = measurement;
        }

        public DatabaseRecord(DateTime date, double? measurement, string room)
        {
            Date = date;
            Measurement = measurement;
            Room = room;
        }

    }
}
