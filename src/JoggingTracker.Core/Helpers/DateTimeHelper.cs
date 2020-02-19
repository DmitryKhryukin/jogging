using System;

namespace JoggingTracker.Core.Helpers
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// returns the first date of the week for the date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="startOfWeek"></param>
        /// <returns></returns>
        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }
}