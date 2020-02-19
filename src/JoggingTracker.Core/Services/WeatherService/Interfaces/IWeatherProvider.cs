using System;
using System.Threading.Tasks;

namespace JoggingTracker.Core.Services.Interfaces
{
    public interface IWeatherProvider
    {
        Task<string> GetWeatherConditionsAsync(DateTime date, double latitude, double longitude);
    }
}