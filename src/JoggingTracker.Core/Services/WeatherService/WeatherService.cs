using System;
using System.Threading;
using System.Threading.Tasks;
using JoggingTracker.Core.Constants;
using JoggingTracker.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace JoggingTracker.Core.Services.WeatherService
{
    public class WeatherService : IWeatherService
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly WeatherServiceSettings _settings;
        private readonly IWeatherProvider _weatherProvider;

        public WeatherService(ILogger<WeatherService> logger,
            WeatherServiceSettings settings,
            IWeatherProvider weatherProvider)
        {
            _logger = logger;
            _settings = settings;
            _weatherProvider = weatherProvider;
        }

        public async Task<string> GetWeatherConditionsAsync(DateTime date, double latitude, double longitude)
        {
            var result = Messages.WatherConditionsAreNotAvailable;

            try
            {
                using (var cts = new CancellationTokenSource(_settings.AllowedTimeout))
                {
                    result = await _weatherProvider
                        .GetWeatherConditionsAsync(date, latitude, longitude)
                        .WaitAsync(cts.Token);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning(ErrorMessages.WeatherProviderTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"{ErrorMessages.WeatherProviderInternalError} : {ex.Message}");
            }

            return result;
        }
    }
}