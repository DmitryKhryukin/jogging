using System;

namespace JoggingTracker.Core.Exceptions
{
    public class JoggingTrackerNotFoundException : Exception
    {
        public JoggingTrackerNotFoundException(string message) : base(message)
        {
        }
    }
}