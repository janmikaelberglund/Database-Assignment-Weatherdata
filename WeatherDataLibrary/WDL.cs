using System;
using System.Globalization;
using System.IO;
using WeatherDataLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WeatherDataLibrary
{
    public class WeatherMethod
    {

        public static List<DatabaseRecord> HotToCold(string sensor)
        {
            List<DatabaseRecord> databaseRecords = new List<DatabaseRecord>();
            using var db = new EFContext();
            var avgTempQuery = db.SensorReadings
                .Where(x => x.Room == sensor)
                .GroupBy(x => x.Date.Date)
                .Select(x => new
                {
                    Date = x.Key,
                    Average = x.Average(x => x.Temperature)
                })
                .OrderByDescending(x => x.Average);

            foreach (var group in avgTempQuery)
            {
                double average = Math.Round((double)group.Average) > -0 ? Math.Round((double)group.Average) : 0;
                databaseRecords.Add(new DatabaseRecord(group.Date, average));
            }

            return databaseRecords;
        }


        public static List<DatabaseRecord> DryToWet(string sensor)
        {
            List<DatabaseRecord> databaseRecords = new List<DatabaseRecord>();
            using var db = new EFContext();
            var avgHumidQuery = db.SensorReadings
                .Where(x => x.Room == sensor)
                .GroupBy(x => x.Date.Date)
                .Select(x => new
                {
                    Date = x.Key,
                    Average = x.Average(x => x.Humidity)
                })
                .OrderBy(x => x.Average);
            foreach (var group in avgHumidQuery)
            {
                double average = Math.Round((double)group.Average) > -0 ? Math.Round((double)group.Average) : 0;
                databaseRecords.Add(new DatabaseRecord(group.Date, average));
            }
            return databaseRecords;
        }

        public static List<DatabaseRecord> MouldRisk(string sensor)
        {
            List<DatabaseRecord> mouldRiskList = new List<DatabaseRecord>();
            using var db = new EFContext();
            var mouldRiskQuery = db.SensorReadings
                .Where(x => x.Room == sensor)
                .GroupBy(x => x.Date.Date)
                .Select(x => new
                {
                    Date = x.Key,
                    AverageRH = x.Average(x => x.Humidity),
                    AverageT = x.Average(x => x.Temperature)
                });

            foreach (var group in mouldRiskQuery)
            {
                double mouldRisk = Math.Round((((double)group.AverageRH - 78) * (double)(group.AverageT / 15) / 0.22));
                if (mouldRisk < 0)
                {
                    mouldRisk = 0;
                }
                else if (mouldRisk > 100)
                {
                    mouldRisk = 100;
                }
                mouldRiskList.Add(new DatabaseRecord(group.Date, mouldRisk));
            }
            return mouldRiskList.OrderByDescending(x => x.Measurement).ToList();
        }

        public static string MeteorologicalCalculator(string startDateString, double temp)
        {
            using var db = new EFContext();

            DateTime startDate = DateTime.Parse(startDateString);

            var q = db.SensorReadings
                .Where(x => x.Room == "Ute" && x.Date >= startDate)
                .GroupBy(x => x.Date.Date)
                .Select(x => new
                {
                    Date = x.Key,
                    avgTemp = Math.Round((double)x.Average(x => x.Temperature), 1)
                })
                .OrderBy(x => x.Date)
                .ToList();


            for (int i = 0; i < q.Count - 5; i++)
            {
                int meteorologicalCount = 0;
                for (int j = 0; j < 5; j++)
                {

                    if (q[i].Date.AddDays(j) == q[i + j].Date && q[i + j].avgTemp < temp)
                    {
                        meteorologicalCount++;
                    }
                    if (meteorologicalCount == 5)
                    {
                        return q[i].Date.ToShortDateString();
                    }
                }
            }
            return null;
        }

        public static string AverageTemperature(string sensor, DateTime date)
        {
            using var db = new EFContext();
            var q = db.SensorReadings
                .Where(x => x.Room == sensor)
                .Where(x => x.Date.Date == date)
                .Average(x => x.Temperature);
            if (q == null)
            {
                return "No measurement matched the searched date.";
            }
            return $"{Math.Round((decimal)q, 1)}°C";
        }

        public static List<string> TempDifference()
        {
            List<string> tempDiff = new List<string>();
            using var db = new EFContext();
            var q = db.SensorReadings
                .GroupBy(x => x.Date.Date)
                .Select(x => new
                {
                    Date = x.Key,
                    InsideAverage = x.Where(x => x.Room == "Inne").Average(x => x.Temperature),
                    OutsideAverage = x.Where(x => x.Room == "Ute").Average(x => x.Temperature)
                })
                .OrderBy(x => (x.InsideAverage - x.OutsideAverage) >= 0 ? (x.InsideAverage - x.OutsideAverage) : -(x.InsideAverage - x.OutsideAverage));

            foreach (var day in q)
            {
                double inside = Math.Round((double)day.InsideAverage, 1);
                double outside = Math.Round((double)day.OutsideAverage, 1);
                double difference = inside - outside;

                difference = difference > 0 ? difference : -difference;

                tempDiff.Add($"{day.Date.Date.ToShortDateString()}\t Innetemperatur: {String.Format("{0:0.0}", inside)}°C \t " +
                    $"Utetemperatur: {String.Format("{0:0.0}", outside)}°C \t Skillnad: {String.Format("{0:0.0}", difference)}°C");
            }
            return tempDiff;
        }

        public static List<BalconyOpen> BalconyOpen()
        {
            List<BalconyOpen> balconyOpen = new List<BalconyOpen>();

            using var db = new EFContext();
            var q = db.SensorReadings
                .OrderBy(x => x.Date)
                .Select(x => new
                {
                    x.Date,
                    x.Temperature,
                    x.Room
                })
                .AsEnumerable()
                .GroupBy(x => x.Date.Date)
                .ToList();


            foreach (var day in q)
            {
                List<DatabaseRecord> inside = day.Where(x => x.Room == "Inne").Select(x => new DatabaseRecord(x.Date, x.Temperature)).ToList();
                List<DatabaseRecord> outside = day.Where(x => x.Room == "Ute").Select(x => new DatabaseRecord(x.Date, x.Temperature)).ToList();
                
                TimeSpan timeOpen = TimeSpan.Zero;
                int addIndex = 0;
                int timesOpened = 0;

                for (int i = 0; i < inside.Count; i++)
                {
                    int count = 0;
                    while (i + count + 2 < inside.Count)
                    {

                        //Nästkommande mätning är lägre än föregående mätning inomhus
                        bool insideTempCondition = inside[i + count].Measurement > inside[i + count + 1].Measurement;

                        //Tidsskillnaden mellan mätningarna får inte vara mer än 10minuter
                        bool timeCondition = inside[i + count + 1].Date - inside[i + count].Date < TimeSpan.Parse("00:10:00");


                        double? outsideFirstTemp = 0;
                        outsideFirstTemp = outside
                            .Where(x => RemoveSeconds(x.Date) == RemoveSeconds(inside[i + count].Date))
                            .Select(x => x.Measurement).FirstOrDefault();

                        double? outsideSecondTemp = 0;
                        outsideSecondTemp = outside
                            .Where(x => RemoveSeconds(x.Date) == RemoveSeconds(inside[i + count + 1].Date))
                            .Select(x => x.Measurement).FirstOrDefault();

                        //Nästkommande mätning är högre eller lika med föregående mätning utomhus
                        bool outsideTempCondition = outsideFirstTemp <= outsideSecondTemp;


                        if (timeCondition && insideTempCondition && outsideTempCondition)
                        {
                            addIndex++;
                        }
                        else
                        {
                            break;
                        }
                        count++;
                    }
                    if (addIndex > 2) //Det måste vara minst tre mätningar i följd som möter mätkravet
                    {
                        timeOpen += (inside[i + addIndex].Date - inside[i].Date);
                        timesOpened++;
                    }
                    i += count;
                    addIndex = 0;
                }
                balconyOpen.Add(new BalconyOpen(inside[0].Date.Date, timeOpen, timesOpened));
            }

            return balconyOpen.OrderByDescending(x => x.TimeOpen).ToList();
        }

        private static DateTime RemoveSeconds(DateTime x)
        {
            return x.Date.AddSeconds(-x.Date.Second);
        }
    }
}
