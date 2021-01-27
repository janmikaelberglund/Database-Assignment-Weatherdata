using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WeatherDataLibrary.Models
{
    public class EFContext : DbContext
    {
        private string connectionString;

        public EFContext() : base()
        {
            var cBuilder = new ConfigurationBuilder();
            cBuilder.AddJsonFile("settings.json", optional: false);
            var configuration = cBuilder.Build();
            connectionString = configuration.GetConnectionString("sqlConnection");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        public DbSet<SensorReading> SensorReadings { get; set; }
    }
}
