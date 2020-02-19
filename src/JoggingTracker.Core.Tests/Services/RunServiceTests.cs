using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore3Mock;
using FluentAssertions;
using JoggingTracker.Core.DTOs.Run;
using JoggingTracker.Core.Exceptions;
using JoggingTracker.Core.Mapping;
using JoggingTracker.Core.Services;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.DataAccess;
using JoggingTracker.DataAccess.DbEntities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace JoggingTracker.Core.Tests.Services
{
    [Collection("Non-Parallel Collection")]
    public class RunServiceTests
    {
        private readonly DbContextMock<JoggingTrackerDataContext> _mockDbContext;
        private readonly TestRunService _runService;
        private readonly Mock<IWeatherService> _mockWeatherService;

        public RunServiceTests()
        {
            DbContextOptions<JoggingTrackerDataContext> dummyOptions = new DbContextOptionsBuilder<JoggingTrackerDataContext>().Options;
            _mockDbContext = new DbContextMock<JoggingTrackerDataContext>(dummyOptions);
            _mockWeatherService = new Mock<IWeatherService>();

            var autoMapperProfile = new AutoMapperProfile();
            var mapperConfiguration = new MapperConfiguration(x => x.AddProfile(autoMapperProfile));
            var mapper = new Mapper(mapperConfiguration);
            
            _runService = new TestRunService(_mockDbContext.Object,
                _mockWeatherService.Object,
                mapper);
        }

        #region CreateRunAsync
        
        [Fact]
        public void CreateRunAsync_SavesRunWithWeatherInfo()
        {
            // arrange
            var request = new RunCreateRequest()
            {
                Date = DateTime.Now,
                Distance = 1,
                Time = 2,
                Latitude = 3,
                Longitude = 4
            };
            var userId = Guid.NewGuid().ToString();
            var forecast = "good weather";

            _mockWeatherService.Setup(x => x.GetWeatherConditionsAsync(request.Date,
                    request.Latitude,
                    request.Longitude))
                .Returns(Task.FromResult(forecast));
            
            // act
            var result = _runService.CreateRunAsync(userId, request).Result;
            
            // assert
            _mockWeatherService.Verify(x => x.GetWeatherConditionsAsync(request.Date,
                request.Latitude,
                request.Longitude), Times.Once);
            
            _mockDbContext.Verify(x => x.AddAsync(It.IsAny<RunDb>(), new CancellationToken()), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(new CancellationToken()), Times.Once);
            
            result.UserId.Should().Be(userId);
            result.Date.Should().Be(request.Date.Date);
            result.Distance.Should().Be(request.Distance);
            result.Time.Should().Be(request.Time);
            result.Latitude.Should().Be(request.Latitude);
            result.Longitude.Should().Be(request.Longitude);
            result.WeatherConditions.Should().Be(forecast);
        }
        
        [Fact]
        public void CreateRunAsync_DbError_ThrowsCustomException()
        {
            // arrange
            var request = new RunCreateRequest()
            {
                Distance = 1,
                Time = 2,
                Latitude = 3,
                Longitude = 4,
                Date = DateTime.Now
            };
            var userId = Guid.NewGuid().ToString();
            var forecast = "good weather";
            var exceptionMessage = "Db error";

            _mockWeatherService.Setup(x => x.GetWeatherConditionsAsync(request.Date,
                request.Latitude,
                request.Longitude)).Returns(Task.FromResult(forecast));
            
            _mockDbContext.Setup(x => x.SaveChangesAsync(new CancellationToken()))
                .Throws(new Exception(exceptionMessage));

            var expectedErrorMessage = $"{ErrorMessages.RunSaveErrorMessage} : {exceptionMessage}";
            
            // act 
            var result  = Assert.ThrowsAsync<JoggingTrackerInternalServerErrorException>(() => _runService.CreateRunAsync(userId, request)).Result;
            
            // assert
            _mockWeatherService.Verify(x => x.GetWeatherConditionsAsync(request.Date,
                request.Latitude,
                request.Longitude), Times.Once);
            
            _mockDbContext.Verify(x => x.AddAsync(It.IsAny<RunDb>(), new CancellationToken()), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(new CancellationToken()), Times.Once);
            
            result.Message.Should().Be(expectedErrorMessage);
        }
        
        #endregion

        #region UpdateRunAsync
        
        [Fact]
        public void UpdateRunAsync_RunDoesntExist_ThrowsNoFoundException()
        {
            // arrange
            var request = new RunUpdateRequest()
            {
                Date = DateTime.Now,
                Distance = 1,
                Time = 2,
                Latitude = 3,
                Longitude = 4
            };
            var userId = Guid.NewGuid().ToString();
            var rundId = 3;

            _mockDbContext.CreateDbSetMock(x => x.Runs, new RunDb[] { });

            // act
            var result = Assert.ThrowsAsync<JoggingTrackerNotFoundException>(
                () => _runService.UpdateRunAsync(userId, rundId, request)).Result ;
            
            // assert
            result.Message.Should().Be(ErrorMessages.RunNotFound);
            
            _mockDbContext.Verify(x => x.Update(It.IsAny<RunDb>()), Times.Never);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            
        }

        [Fact]
        public void UpdateRunAsync_CorrectRunId_IncorrectUserId_ThrowsNoFoundException()
        {
            // arrange
            var request = new RunUpdateRequest()
            {
                Date = DateTime.Now,
                Distance = 1,
                Time = 2,
                Latitude = 3,
                Longitude = 4
            };
            var userId = Guid.NewGuid().ToString();
            var rundId = 3;

            var dbRuns = new RunDb[]
            {
                new RunDb()
                {
                    Id = rundId,
                    UserId = Guid.NewGuid().ToString()
                }
            };

            _mockDbContext.CreateDbSetMock(x => x.Runs, dbRuns);

            // act
            var result = Assert.ThrowsAsync<JoggingTrackerNotFoundException>(
                () => _runService.UpdateRunAsync(userId, rundId, request)).Result ;
            
            // assert
            result.Message.Should().Be(ErrorMessages.RunNotFound);
            
            _mockDbContext.Verify(x => x.Update(It.IsAny<RunDb>()), Times.Never);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public void UpdateRunAsync_LocationAndDateAreNotChanged_RunIsUpdated_WeatherServiceIsNotCalled()
        {
            // arrange
            var date = DateTime.Now;
            var latitude = 3.44;
            var longitude = 67.456;

            var request = new RunUpdateRequest()
            {
                Date = date,
                Distance = 1,
                Time = 2,
                Latitude = latitude,
                Longitude = longitude
            };
            var userId = Guid.NewGuid().ToString();
            var rundId = 3;

            var dbRuns = new RunDb[]
            {
                new RunDb()
                {
                    Id = rundId,
                    UserId = userId,
                    Date = date,
                    Latitude = latitude,
                    Longitude = longitude,
                    WeatherConditions = "good forecast"
                }
            };

            _mockDbContext.CreateDbSetMock(x => x.Runs, dbRuns);

            // act
            var result = _runService.UpdateRunAsync(userId, rundId, request).Result ;
            
            // assert
            result.Id.Should().Be(rundId);
            result.UserId.Should().Be(userId);
            result.Date.Should().Be(dbRuns[0].Date.Date);
            result.Distance.Should().Be(request.Distance);
            result.Time.Should().Be(request.Time);
            result.Latitude.Should().Be(dbRuns[0].Latitude);
            result.Longitude.Should().Be(dbRuns[0].Longitude);
            result.WeatherConditions.Should().Be(dbRuns[0].WeatherConditions);
            
            _mockWeatherService.Verify(x => x.GetWeatherConditionsAsync(
                It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
            
            _mockDbContext.Verify(x => x.Update(It.IsAny<RunDb>()), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void UpdateRunAsync_LocationUpdated_RunIsUpdated_WeatherServiceIsCalled()
        {
            // arrange
            var date = DateTime.Now;
            var latitude = 3.44;
            var longitude = 67.456;
            
            var newDate = new DateTime(1999, 1, 2);
            var newLatitude = 56.44;
            var newLongitude = 34.456;
            var newForecast = "new forecast";

            var request = new RunUpdateRequest()
            {
                Date = newDate,
                Distance = 1,
                Time = 2,
                Latitude = newLatitude,
                Longitude = newLongitude
            };
            var userId = Guid.NewGuid().ToString();
            var rundId = 3;

            var dbRuns = new RunDb[]
            {
                new RunDb()
                {
                    Id = rundId,
                    UserId = userId,
                    Date = date,
                    Latitude = latitude,
                    Longitude = longitude,
                    WeatherConditions = "good forecast"
                }
            };

            _mockDbContext.CreateDbSetMock(x => x.Runs, dbRuns);
            
            _mockWeatherService.Setup(x => x.GetWeatherConditionsAsync(
                newDate, newLatitude, newLongitude)).Returns(Task.FromResult(newForecast));

            // act
            var result = _runService.UpdateRunAsync(userId, rundId, request).Result ;
            
            // assert
            result.Id.Should().Be(rundId);
            result.UserId.Should().Be(userId);
            result.Date.Should().Be(request.Date);
            result.Distance.Should().Be(request.Distance);
            result.Time.Should().Be(request.Time);
            result.Latitude.Should().Be(request.Latitude);
            result.Longitude.Should().Be(request.Longitude);
            result.WeatherConditions.Should().Be(newForecast);
            
            _mockWeatherService.Verify(x => x.GetWeatherConditionsAsync(
                newDate, newLatitude, newLongitude), Times.Once);
            
            _mockDbContext.Verify(x => x.Update(It.IsAny<RunDb>()), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public void UpdateRunAsync_InternalServerExceptionOnUpdate_ThrowsException()
        {
            // arrange
            var date = DateTime.Now;
            var latitude = 3.44;
            var longitude = 67.456;

            var request = new RunUpdateRequest()
            {
                Date = date,
                Distance = 1,
                Time = 2,
                Latitude = latitude,
                Longitude = longitude
            };
            var userId = Guid.NewGuid().ToString();
            var rundId = 3;

            var dbRuns = new RunDb[]
            {
                new RunDb()
                {
                    Id = rundId,
                    UserId = userId,
                    Date = date,
                    Latitude = latitude,
                    Longitude = longitude,
                    WeatherConditions = "good forecast"
                }
            };

            _mockDbContext.CreateDbSetMock(x => x.Runs, dbRuns);

            var errorMessage = "error message";
            _mockDbContext.Setup(x => x.Update(It.IsAny<RunDb>()))
                .Throws(new Exception(errorMessage));
            
            // act
            var result = Assert.ThrowsAsync<JoggingTrackerInternalServerErrorException>(
                () => _runService.UpdateRunAsync(userId, rundId, request)).Result ;
            
            // assert
            var expectedErrorMessage = $"{ErrorMessages.RunSaveErrorMessage} : {errorMessage}";
            result.Message.Should().Be(expectedErrorMessage);
            
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        #endregion

        #region IsLocationOrDateUpdated

        [Fact]
        public void IsLocationOrDateUpdated_DateIsUpdated_ReturnsTrue()
        {
            // arrange
            var latitude = 0.45;
            var longitude = 0.67;
            var date = new DateTime(2020, 02, 02);
            var newDate = new DateTime(2010, 01, 01);
            
            var runDb = new RunDb()
            {
                Latitude = latitude,
                Longitude = longitude,
                Date = date
            };

            var request = new RunUpdateRequest()
            {
                Latitude = latitude,
                Longitude = longitude,
                Date = newDate
            };
            
            // act
            var result = _runService.IsLocationOrDateUpdated(runDb, request);
            
            // assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public void IsLocationOrDateUpdated_LocationAndDateAreTheSame_ReturnsFalse()
        {
            // arrange
            var latitude = 0.45;
            var longitude = 0.67;
            var date = new DateTime(2020, 02, 02);
            
            var runDb = new RunDb()
            {
                Latitude = latitude,
                Longitude = longitude,
                Date = date
            };

            var request = new RunUpdateRequest()
            {
                Latitude = latitude,
                Longitude = longitude,
                Date = date
            };
            
            // act
            var result = _runService.IsLocationOrDateUpdated(runDb, request);
            
            // assert
            result.Should().BeFalse();
        }
        
        [Fact]
        public void IsLocationOrDateUpdated_LatitudeDifferenceMoreThanTolerance_ReturnsTrue()
        {
            // arrange
            var latitude = 0.45;
            var newLatidude = 0.4501; // LocationTolerance = 0.0001
            var longitude = 0.67;
            var date = new DateTime(2020, 02, 02);
            
            var runDb = new RunDb()
            {
                Latitude = latitude,
                Longitude = longitude,
                Date = date
            };

            var request = new RunUpdateRequest()
            {
                Latitude = latitude,
                Longitude = newLatidude,
                Date = date
            };
            
            // act
            var result = _runService.IsLocationOrDateUpdated(runDb, request);
            
            // assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public void IsLocationOrDateUpdated_LongitudeDifferenceMoreThanTolerance_ReturnsTrue()
        {
            // arrange
            var latitude = 0.45;
            var longitude = 0.67;
            var newLongitude = 0.6701; // LocationTolerance = 0.0001
            var date = new DateTime(2020, 02, 02);
            
            var runDb = new RunDb()
            {
                Latitude = latitude,
                Longitude = longitude,
                Date = date
            };

            var request = new RunUpdateRequest()
            {
                Latitude = newLongitude,
                Longitude = latitude,
                Date = date
            };
            
            // act
            var result = _runService.IsLocationOrDateUpdated(runDb, request);
            
            // assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public void IsLocationOrDateUpdated_LongitudeAndLatitudeDifferencesLessThanTolerance_ReturnsFalse()
        {
            // arrange
            var latitude = 0.45;
            var longitude = 0.67;
            var newLatitude = 0.45009; // LocationTolerance = 0.0001
            var newLongitude = 0.67009; // LocationTolerance = 0.0001
            var date = new DateTime(2020, 02, 02);
            
            var runDb = new RunDb()
            {
                Latitude = latitude,
                Longitude = longitude,
                Date = date
            };

            var request = new RunUpdateRequest()
            {
                Latitude = newLatitude,
                Longitude = newLongitude,
                Date = date
            };
            
            // act
            var result = _runService.IsLocationOrDateUpdated(runDb, request);
            
            // assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeleteRunAsync

        [Fact]
        public void DeleteRunAsync_RunIdDosntExist_ThrowsNoFoundException()
        {
            // arrange
            var userId = Guid.NewGuid().ToString();
            var rundId = 3;
            
            _mockDbContext.CreateDbSetMock(x => x.Runs, new RunDb[] { });

            // act
            var result = Assert.ThrowsAsync<JoggingTrackerNotFoundException>(
                () => _runService.DeleteRunAsync(userId, rundId)).Result;
            
            // assert
            result.Message.Should().Be(ErrorMessages.RunNotFound);
            
            _mockDbContext.Verify(x => x.Remove(It.IsAny<RunDb>()), Times.Never);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public void DeleteRunAsync_RunIdExists_UserIdIsIncorrect_ThrowsNoFoundException()
        {
            // arrange
            var userId = Guid.NewGuid().ToString();
            var rundId = 3;

            var runs = new RunDb[]
            {
                new RunDb()
                {
                    Id = rundId,
                    UserId = Guid.NewGuid().ToString()
                }
            };
            
            _mockDbContext.CreateDbSetMock(x => x.Runs, runs);

            // act
            var result = Assert.ThrowsAsync<JoggingTrackerNotFoundException>(
                () => _runService.DeleteRunAsync(userId, rundId)).Result;
            
            // assert
            result.Message.Should().Be(ErrorMessages.RunNotFound);
            
            _mockDbContext.Verify(x => x.Remove(It.IsAny<RunDb>()), Times.Never);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async void DeleteRunAsync_RecordExists_RemoveRecord()
        {
            // arrange
            var userId = Guid.NewGuid().ToString();
            var rundId = 3;

            var runs = new RunDb[]
            {
                new RunDb()
                {
                    Id = rundId,
                    UserId = userId
                }
            };
            
            _mockDbContext.CreateDbSetMock(x => x.Runs, runs);

            // act
            await _runService.DeleteRunAsync(userId, rundId);
            
            // assert
            _mockDbContext.Verify(x => x.Remove(It.IsAny<RunDb>()), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public void DeleteRunAsync_InternaServerError_ThrowsCustomException()
        {
            // arrange
            var userId = Guid.NewGuid().ToString();
            var rundId = 3;

            var runs = new RunDb[]
            {
                new RunDb()
                {
                    Id = rundId,
                    UserId = userId
                }
            };
            
            _mockDbContext.CreateDbSetMock(x => x.Runs, runs);

            var exceptionMessage = "exception message";
            _mockDbContext.Setup(x => x.Remove(It.IsAny<RunDb>()))
                .Throws(new Exception(exceptionMessage));

            // act
            var result = Assert.ThrowsAsync<JoggingTrackerInternalServerErrorException>(
                () => _runService.DeleteRunAsync(userId, rundId)).Result ;
            
            // assert
            var expectedErrorMessage = $"{ErrorMessages.RunDeleteErrorMessage} : {exceptionMessage}";
            result.Message.Should().Be(expectedErrorMessage);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region GetRunsAsync

        [Fact]
        public void GetRunsAsync_NoFilter_ReturnsAllRunsForProviderUser()
        {
            // arrange
            var userId = Guid.NewGuid().ToString();
            
            var runs = new[]
            {
                new RunDb()
                {
                    Id = 1,
                    UserId = userId
                },
                new RunDb()
                {
                    Id = 2,
                    UserId = userId
                },
                new RunDb()
                {
                    Id = 3,
                    UserId = Guid.NewGuid().ToString()
                }
            };
            
            _mockDbContext.CreateDbSetMock(x => x.Runs, runs);
            
            // act
            var result = _runService.GetRunsAsync(userId).Result;

            // assert
            result.Items.Count().Should().Be(2);
            result.Total.Should().Be(2);
            result.PageNumber.Should().BeNull();
            result.PageSize.Should().BeNull();
        }
        
        [Fact]
        public void GetRunsAsync_IncorrectFilter_ThrowsException()
        {
            // arrange
            var runs = new RunDb[] { };
            _mockDbContext.CreateDbSetMock(x => x.Runs, runs);
        
            var filter = "password = '1234'";
            
            // act
            var result = Assert.ThrowsAsync<JoggingTrackerBadRequestException>(
                () => _runService.GetRunsAsync(Guid.NewGuid().ToString(), filter)).Result;
        
            // assert
            result.Message.Should().StartWith(ErrorMessages.CouldntParseFilter);
        }
        
        [Fact]
        public void GetRunsAsync_CorrectFilter_ReturnFilteredRuns()
        {
            // arrange
            var userId = Guid.NewGuid().ToString();
            var date = new DateTime(2016, 5, 1); // 2016-05-01
            
            var runs = new[]
            {
                new RunDb()
                {
                    Id = 1,
                    UserId = userId,
                    Date = date,
                    Distance = 21
                },
                new RunDb()
                {
                    Id = 2,
                    UserId = userId,
                    Date = date,
                    Distance = 9
                },
                new RunDb()
                {
                    Id = 3,
                    UserId = Guid.NewGuid().ToString(), // different user id,
                    Date = date,
                    Distance = 9
                },
                new RunDb()
                {
                    Id = 4,
                    UserId = userId,
                    Date = date,
                    Distance = 15 // filtered by distance
                }
                ,
                new RunDb()
                {
                    Id = 5,
                    UserId = userId,
                    Date = new DateTime(2020,02,02), // filtered by date
                    Distance = 9 
                }
            };
            
            _mockDbContext.CreateDbSetMock(x => x.Runs, runs);
        
            var filter = "(date eq '2016-05-01') AND ((distance gt 20) OR (distance lt 10))";

            // act
            var result =  _runService.GetRunsAsync(userId, filter).Result;
        
            // assert
            result.Items.Count().Should().Be(2);
            result.Total.Should().Be(2);
            result.PageNumber.Should().BeNull();
            result.PageSize.Should().BeNull();
        }
        
        [Fact]
        public void GetRunsAsync_PageSizeAndPageNumberProvided_ReturnPagedRuns()
        {
            // arrange
            var userId = Guid.NewGuid().ToString();
            var date = new DateTime(2016, 5, 1); // 2016-05-01
            
            var runs = new[]
            {
                new RunDb()
                {
                    Id = 1,
                    UserId = userId,
                    Date = date,
                    Distance = 21
                },
                new RunDb()
                {
                    Id = 2,
                    UserId = userId,
                    Date = date,
                    Distance = 9
                },
                new RunDb()
                {
                    Id = 3,
                    UserId = Guid.NewGuid().ToString(), // filtered by user
                    Date = date,
                    Distance = 9
                },
                new RunDb()
                {
                    Id = 4,
                    UserId = userId,
                    Date = date,
                    Distance = 15
                }
                ,
                new RunDb()
                {
                    Id = 5,
                    UserId = userId,
                    Date = new DateTime(2020,02,02),
                    Distance = 9 
                }
                ,
                new RunDb()
                {
                    Id = 6,
                    UserId = userId,
                    Date = new DateTime(2020,02,02),
                    Distance = 9 
                }
            };
            
            _mockDbContext.CreateDbSetMock(x => x.Runs, runs);

            var pageSize = 2;
            var pageNumber = 2;

            // act
            var result =  _runService.GetRunsAsync(userId, null, pageNumber, pageSize).Result;
        
            // assert
            result.Items.Count().Should().Be(2);
            result.Total.Should().Be(5);
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);
        }

        #endregion

        #region GetWeeksReportAsync

        [Fact]
        public void GetWeeksReportAsync_ReturnsAverageSpeedAndDistanceByWeeksForUser()
        {
            var userId = Guid.NewGuid().ToString();
            var differentUserId = Guid.NewGuid().ToString();
            
            // arrange
            List<RunDb> runs = new List<RunDb>
            {
                new RunDb()
                {
                    Id = 1,
                    UserId = userId,
                    Distance = 500,
                    Time = 300,
                    Date = new DateTime(2020, 2, 11)
                },
                new RunDb()
                {
                    Id = 2,
                    UserId = userId,
                    Distance = 300,
                    Time = 500,
                    Date = new DateTime(2020, 2, 12)
                },
                new RunDb()
                {
                    Id = 3,
                    UserId = userId,
                    Distance = 300,
                    Time = 500,
                    Date = new DateTime(2019, 2, 12)
                },
                new RunDb()
                {
                    Id = 4,
                    UserId = differentUserId,
                    Distance = 100,
                    Time = 500,
                    Date = new DateTime(2019, 2, 12)
                }
            };
            
            _mockDbContext.CreateDbSetMock(x => x.Runs, runs);

            var result = _runService.GetWeeksReportAsync(userId).Result;

            result.Should().NotBeNull();
            result.Items.Count().Should().Be(2); // 2 different weeks
            result.Items.ElementAt(0).WeekStartDate.Should().Be(new DateTime(2020, 2, 10)); //monday
            result.Items.ElementAt(0).AverageDistance.Should().Be(400); // 300 + 500 / 2
            result.Items.ElementAt(0).AverageSpeed.Should().Be(3.6); // (500 + 300)/(300+500)*3.6
            result.Items.ElementAt(1).WeekStartDate.Should().Be(new DateTime(2019, 2, 11)); //monday
            result.Items.ElementAt(1).AverageDistance.Should().Be(300); // 300
            result.Items.ElementAt(1).AverageSpeed.Should().Be(2.16); // (300)/(500)*3.6
        }


        #endregion
    }

    class TestRunService : RunService
    {
        public TestRunService(JoggingTrackerDataContext dbContext, IWeatherService weatherService, IMapper mapper) : base(dbContext, weatherService, mapper)
        {
        }
        public new bool IsLocationOrDateUpdated(RunDb runDb, RunUpdateRequest request)
        {
            return base.IsLocationOrDateUpdated(runDb, request);
        }
    }
}