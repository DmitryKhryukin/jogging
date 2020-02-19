using System;

namespace JoggingTracker.Core.Exceptions
{
    public class JoggingTrackerInternalServerErrorException : Exception
    {
        public JoggingTrackerInternalServerErrorException(string message) : base(message)
        {
        }
    }
}