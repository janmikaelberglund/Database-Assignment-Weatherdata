using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeatherDataLibrary.Models;

namespace Database_Assignment_Weatherdata
{
    class Program
    {
        public static CancellationTokenSource tokenSource = new CancellationTokenSource();
        public static CancellationToken ct = tokenSource.Token;

        static void Main(string[] args)
        {

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "TemperaturData.csv");
            List<SensorReading> dataToImport = new List<SensorReading>();

            MenuLayer1(filePath, dataToImport);
        }

        private static void MenuLayer1(string filePath, List<SensorReading> dataToImport)
        {
            Console.WriteLine("1: Import records into database(first run only).");
            Console.WriteLine("2: Read database entries.");
            Console.CursorVisible = false;
            while (true)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1: //Importerar csv-filen till databasens tabell
                    case ConsoleKey.NumPad1:
                        Console.Clear();
                        DateTime startTime = DateTime.Now;
                        ImportFileData(filePath, dataToImport);
                        InsertImportedData(dataToImport);
                        TimeSpan elapsedTime = DateTime.Now - startTime;
                        Console.WriteLine($"Task completed in {Math.Round(elapsedTime.TotalSeconds)} seconds.");
                        goto case ConsoleKey.D2;

                    case ConsoleKey.D2: //Menyn för queries av databasen
                    case ConsoleKey.NumPad2:
                        MenuLayer2();
                        break;
                    default:
                        break;
                }
            }
        }

        private static void MenuLayer2()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("1: Average temperature.");
                Console.WriteLine("2: Meteorological fall and winter.");
                Console.WriteLine("3: Average temperatures outside.");
                Console.WriteLine("4: Average temperatures inside.");
                Console.WriteLine("5: Average humidity outside.");
                Console.WriteLine("6: Average humidity inside.");
                Console.WriteLine("7: Mould risk outside.");
                Console.WriteLine("8: Mould risk inside.");
                Console.WriteLine("9: Balcony open.");
                Console.WriteLine("0: Temperature difference inside and outside");
                Console.WriteLine();

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        Console.Clear();
                        Console.WriteLine("Ange datum(yyyy-mm-dd): ");
                        bool running = true;
                        while (running)
                        {
                            bool test = DateTime.TryParse(Console.ReadLine(), out DateTime date);
                            if (test)
                            {
                                Console.WriteLine($"Average temperature inside: {WeatherDataLibrary.WeatherMethod.AverageTemperature("Inne", date)}");
                                Console.WriteLine($"Average temperature outside: {WeatherDataLibrary.WeatherMethod.AverageTemperature("Ute", date)}");
                                running = false;
                            }
                            else
                            {
                                Console.WriteLine("Felaktigt format, försök igen:");
                            }
                        }
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        Console.Clear();
                        string fall = WeatherDataLibrary.WeatherMethod.MeteorologicalCalculator("2016-08-01", 10);
                        if (fall != null)
                        {
                            Console.WriteLine($"Meteoroligical fall started: {fall}");
                        }
                        else
                        {
                            Console.WriteLine($"The conditions for meteorological fall were not met");
                        }
                        string winter = WeatherDataLibrary.WeatherMethod.MeteorologicalCalculator("2016-08-01", 0.1);
                        if (winter != null)
                        {
                            Console.WriteLine($"Meteoroligical fall started: {winter}");
                        }
                        else
                        {
                            Console.WriteLine($"The conditions for meteorological winter were not met");
                        }
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        Console.Clear();
                        foreach (var day in WeatherDataLibrary.WeatherMethod.HotToCold("Ute"))
                        {
                            Console.WriteLine($"{day.Date.ToShortDateString()}: {day.Measurement}°C");
                        }
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        Console.Clear();
                        foreach (var day in WeatherDataLibrary.WeatherMethod.HotToCold("Inne"))
                        {
                            Console.WriteLine($"{day.Date.ToShortDateString()}: {day.Measurement}°C");
                        }
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        Console.Clear();
                        foreach (var day in WeatherDataLibrary.WeatherMethod.DryToWet("Ute"))
                        {
                            Console.WriteLine($"{day.Date.ToShortDateString()}: {day.Measurement}% RH");
                        }
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        Console.Clear();
                        foreach (var day in WeatherDataLibrary.WeatherMethod.DryToWet("Inne"))
                        {
                            Console.WriteLine($"{day.Date.ToShortDateString()}: {day.Measurement}% RH");
                        }
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        Console.Clear();
                        foreach (var mouldRisk in WeatherDataLibrary.WeatherMethod.MouldRisk("Ute"))
                        {
                            Console.WriteLine($"{mouldRisk.Date.ToShortDateString()}: {mouldRisk.Measurement}% risk of mould.");
                        }
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D8:
                    case ConsoleKey.NumPad8:
                        Console.Clear();
                        foreach (var mouldRisk in WeatherDataLibrary.WeatherMethod.MouldRisk("Inne"))
                        {
                            Console.WriteLine($"{mouldRisk.Date.ToShortDateString()}: {mouldRisk.Measurement}% risk of mould.");
                        }
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D9:
                    case ConsoleKey.NumPad9:
                        Console.Clear();
                        foreach (var day in WeatherDataLibrary.WeatherMethod.BalconyOpen())
                        {
                            if (day.TimesOpened != 0)
                            {
                                Console.WriteLine($"Den {day.Date.ToShortDateString()} var balkongen öppen " +
                                    $"{day.TimeOpen.Hours} timmar, {day.TimeOpen.Minutes} minuter och {day.TimeOpen.Seconds} sekunder. " +
                                    $"Dörren öppnades: {day.TimesOpened}ggr");
                            }
                        }
                        Console.ReadKey(true);
                        break;

                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        Console.Clear();
                        foreach (var reading in WeatherDataLibrary.WeatherMethod.TempDifference())
                        {
                            Console.WriteLine(reading);
                        }
                        Console.ReadKey(true);
                        break;

                    default:
                        break;
                }
            }
        }

        



        private static void ImportFileData(string filePath, List<SensorReading> readings)
        {
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            Console.WriteLine("Reading file...");
            using (StreamReader sr = new StreamReader(filePath))
            {
                string dataEntry;
                while ((dataEntry = sr.ReadLine()) != null)
                {
                    SensorReading reading = new SensorReading();
                    string[] splitEntry = dataEntry.Split(',');

                    //Datum
                    string dateEntry = splitEntry[0];
                    try
                    {
                        reading.Date = DateTime.ParseExact(dateEntry, dateFormat, CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        continue;
                    }
                    

                    //Rum
                    reading.Room = splitEntry[1];

                    //Temperatur
                    _ = double.TryParse(splitEntry[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double resultTemp);
                   reading.Temperature = resultTemp;

                    //Luftfuktighet
                    _ = int.TryParse(splitEntry[3], out int resultHumid);
                    reading.Humidity = resultHumid;

                    readings.Add(reading);
                }
            }
            Console.WriteLine("Done.");
        }

        private static void InsertImportedData(List<SensorReading> readings)
        {
            int listLenght = readings.Count;
            int count = 0;
            Console.WriteLine("Reading records:");

            using var db = new WeatherDataLibrary.Models.EFContext();
            foreach (var record in readings)
            {
                db.Add(record);
                Console.SetCursorPosition(0, 3);
                count++;
                Console.Write($"{count} out of {listLenght} complete.");
            }
            Console.WriteLine("\r\nWriting to database. This might take a while");

            Task saveChanges = Task.Run( () => db.SaveChanges());
            Task animationTask = Task.Run(() => Animation(), tokenSource.Token);

            Task.WaitAll(saveChanges);
            tokenSource.Cancel();
            Thread.Sleep(1);

            Console.WriteLine("\bWriting to database complete.");
        }

        private static void Animation()
        {
            int count = 0;
            //Console.Write("|");
            while (true)
            {
                Thread.Sleep(100);
                if (count == 0)
                {
                    Console.Write("/");
                    count++;
                }
                else if (count == 1)
                {
                    Console.Write("-");
                    count++;
                }
                else if (count == 2)
                {
                    Console.Write("\\");
                    count++;
                }
                else
                {
                    Console.Write("|");
                    count = 0;
                }
                Console.Write("\b");
                if (ct.IsCancellationRequested)
                {
                    Console.Write("\b");
                    return;
                }
            }
        }
    }
}
