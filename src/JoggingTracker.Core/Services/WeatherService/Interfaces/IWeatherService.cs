using System;
using System.Threading.Tasks;

namespace JoggingTracker.Core.Services.Interfaces
{
    public interface IWeatherService
    {
        Task<string> GetWeatherConditionsAsync(DateTime date, double latitude, double longitude);
    }
}