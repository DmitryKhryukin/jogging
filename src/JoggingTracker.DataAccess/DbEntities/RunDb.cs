using System;

namespace JoggingTracker.DataAccess.DbEntities
{
    public class RunDb
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// in meters
        /// </summary>
        public int Distance { get; set; }

        /// <summary>
        /// in seconds
        /// </summary>
        public int Time { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string WeatherConditions { get; set; }
    }
}