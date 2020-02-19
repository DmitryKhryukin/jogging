using System;

namespace JoggingTracker.Core.Exceptions
{
    public class JoggingTrackerBadRequestException : Exception
    {
        public JoggingTrackerBadRequestException(string message) : base(message)
        {
        }
    }
}