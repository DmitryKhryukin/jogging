using System;

namespace JoggingTracker.Core.DTOs.Run
{
    // cannot user base class because of filter/predicate conversion logic
    public class RunDto
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