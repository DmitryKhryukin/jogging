using System;
using System.Threading.Tasks;
using DarkSky.Models;
using DarkSky.Services;
using JoggingTracker.Core.Constants;
using JoggingTracker.Core.Services.Interfaces;

namespace JoggingTracker.Core.Services.WeatherService
{
    public class DarkSkyWeatherProvider : IWeatherProvider
    {
        private readonly DarkSkyService _darkSkyService;

        public DarkSkyWeatherProvider(WeatherServiceSettings settings)
        {
            _darkSkyService = new DarkSkyService(settings.ApiKey);
        }
        
        public DarkSkyWeatherProvider(DarkSkyService darkSkyService)
        {
            _darkSkyService = darkSkyService;
        }
        
        public async Task<string> GetWeatherConditionsAsync(DateTime date, double latitude, double longitude)
        {
            var result = Messages.WatherConditionsAreNotAvailable;

            var optionalParameters = new OptionalParameters() {ForecastDateTime = date};
            var forecast = await _darkSkyService.GetForecast(latitude, longitude, optionalParameters);

            if (forecast?.IsSuccessStatus == true)
            {
                result = forecast.Response.Currently.Summary;
            }
            
            return result;
        }
    }
}