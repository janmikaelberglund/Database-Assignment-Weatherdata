using System;
namespace WeatherDataLibrary.Models
{
    public class SensorReading
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Room { get; set; }
        public double? Temperature { get; set; }
        public int? Humidity { get; set; }
    }
}
