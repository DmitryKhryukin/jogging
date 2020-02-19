using System;
using System.ComponentModel.DataAnnotations;

namespace JoggingTracker.Core.DTOs.Run
{
    public class RunUpdateRequest
    {
        [Required(ErrorMessage = ErrorMessages.DateIsRequired)]
        public DateTime Date { get; set; }

        /// <summary>
        /// in meters
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.DistanceIsRequired)]
        public int Distance { get; set; }

        /// <summary>
        /// in seconds
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.TimeIsRequired)]
        public int Time { get; set; }
        
        [Range(-90, 90, ErrorMessage = ErrorMessages.LatitudeValue)]
        [Required(ErrorMessage = ErrorMessages.LatitudeIsRequired)]
        public double Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = ErrorMessages.LongitudeValue)]
        [Required(ErrorMessage = ErrorMessages.LongitudeIsRequired)]
        public double Longitude { get; set; }
    }
}