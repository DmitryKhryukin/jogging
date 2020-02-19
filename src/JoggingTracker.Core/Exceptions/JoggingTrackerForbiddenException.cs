using System;

namespace JoggingTracker.Core.Exceptions
{
    public class JoggingTrackerForbiddenException : Exception
    {
        public JoggingTrackerForbiddenException(string message) : base(message)
        {
        }
    }
}