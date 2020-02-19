using System;

namespace JoggingTracker.Core.DTOs.Run
{
    public class RunsWeeksReport
    {
        public DateTime WeekStartDate { get; set; }
        public double AverageSpeed { get; set; } // kms per hour
        public double AverageDistance { get; set; } // meters
    }
}