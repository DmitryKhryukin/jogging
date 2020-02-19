using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JoggingTracker.Core.Constants;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.Core.Services.WeatherService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Priority;

namespace JoggingTracker.Core.Tests.Services
{
    [Collection("Non-Parallel Collection")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [DefaultPriority(0)]
    public class WeatherServiceTests
    {
        private readonly WeatherService _weatherService;
        private readonly Mock<IWeatherProvider> _mockWeatherProvider;
        private readonly Mock<AbstractLogger<WeatherService>> _mockLogger;

        private readonly WeatherServiceSettings _weatherServiceSettings = new WeatherServiceSettings()
        {
            ApiKey = "testKey",
            AllowedTimeout = 100
        };
        
        public WeatherServiceTests()
        {
            _mockWeatherProvider = new Mock<IWeatherProvider>();

            _mockLogger = new Mock<AbstractLogger<WeatherService>>();
            
            _weatherService = new WeatherService(
                _mockLogger.Object,
                _weatherServiceSettings,
                _mockWeatherProvider.Object);
        }

        [Fact, Priority(1)]
        public void GetWeatherConditionsAsync_WeatherProviderThrowsException_ReturnsDefaultMessage_LogsWarning()
        {
            // arrange
            var exceptionMessage = "test exception";

            var date = DateTime.Now;
            double latitude = 5;
            double longitude = 6;
            
            _mockWeatherProvider.Setup(x => x.GetWeatherConditionsAsync(date,
                latitude,
                longitude)).Throws(new Exception(exceptionMessage));
            
            var expectedErrorMessage = $"{ErrorMessages.WeatherProviderInternalError} : {exceptionMessage}";
            
            // act
            var result = _weatherService.GetWeatherConditionsAsync(date, latitude, longitude).Result;

            result.Should().Be(Messages.WatherConditionsAreNotAvailable);
            _mockWeatherProvider.Verify(x => x.GetWeatherConditionsAsync(date,
                latitude,
                longitude), Times.Once);
            
            _mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<Exception>(), 
                expectedErrorMessage), Times.Once);

        }

        [Fact, Priority(2)]
        public void GetWeatherConditionsAsync_WeatherProviderTimeout_ReturnsDefaultMessage_LogsWarning()
        {
            // arrange 
            var date = DateTime.Now;
            double latitude = 5;
            double longitude = 6;
            
            int notAllowedTimeout = _weatherServiceSettings.AllowedTimeout + 100;
            
            _mockWeatherProvider.Setup(x => x.GetWeatherConditionsAsync(date,
                    latitude,
                    longitude))
                .Returns(() =>
                {
                    Thread.Sleep(notAllowedTimeout);
                    return Task.FromResult("good forecast");
                });
            
            // act 
            var result = _weatherService.GetWeatherConditionsAsync(date, latitude, longitude).Result;
            
            // assert
            _mockWeatherProvider.Verify(x => x.GetWeatherConditionsAsync(date,
                latitude,
                longitude), Times.Once);

            _mockLogger.Verify(x => 
                x.Log(LogLevel.Warning, It.IsAny<Exception>(), ErrorMessages.WeatherProviderTimeout), Times.Once);

            result.Should().Be(Messages.WatherConditionsAreNotAvailable);
        }
        
        [Fact, Priority(3)]
        public void GetWeatherConditionsAsync_WeatherProviderReturnsWeatherConditions_ReturnsWeatherConditions()
        {
            // arrange 
            var date = DateTime.Now;
            double latitude = 5;
            double longitude = 6;
            var forecast = "good forecast";
            
            _mockWeatherProvider.Setup(x => x.GetWeatherConditionsAsync(date,
                    latitude,
                    longitude))
                .Returns(Task.FromResult(forecast));
            
            // act 
            var result = _weatherService.GetWeatherConditionsAsync(date, latitude, longitude).Result;
            
            // assert
            _mockWeatherProvider.Verify(x => x.GetWeatherConditionsAsync(date,
                latitude,
                longitude), Times.Once);

            _mockLogger.Verify(x => 
                x.Log(It.IsAny<LogLevel>(), It.IsAny<Exception>(), ErrorMessages.WeatherProviderTimeout), Times.Never);

            result.Should().Be(forecast);
        }
    }
}