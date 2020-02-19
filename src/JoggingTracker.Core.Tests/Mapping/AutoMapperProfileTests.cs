using System;
using AutoMapper;
using FluentAssertions;
using JoggingTracker.Core.DTOs.Run;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.Core.Mapping;
using JoggingTracker.DataAccess.DbEntities;
using Xunit;

namespace JoggingTracker.Core.Tests.Mapping
{
    public class AutoMapperProfileTests
    {
        private readonly Mapper _mapper;

        public AutoMapperProfileTests()
        {
            var autoMapperProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(x => x.AddProfile(autoMapperProfile));
            _mapper = new Mapper(configuration);
        }

        #region Users

        
        [Fact]
        public void From_UserDb_To_UserDto()
        {
            var userDb = new UserDb()
            {
                Id =  Guid.NewGuid().ToString(),
                UserName = "test"
            };

            var userDto = _mapper.Map<UserDb, UserDto>(userDb);

            userDto.Id.Should().Be(userDb.Id);
            userDto.UserName.Should().Be(userDb.UserName);
        }
        
        [Fact]
        public void From_UserDb_To_UserWithRolesDto()
        {
            var userDb = new UserDb()
            {
                Id =  Guid.NewGuid().ToString(),
                UserName = "test"
            };

            var userWithRolesDto = _mapper.Map<UserDb, UserWithRolesDto>(userDb);

            userWithRolesDto.Id.Should().Be(userDb.Id);
            userWithRolesDto.UserName.Should().Be(userDb.UserName);
        }

        #endregion
        
        #region Runs

        [Fact]
        public void From_CreateRunRequest_To_RunDb()
        {
            var createRunRequest = new RunCreateRequest()
            {
                Date = new DateTime(2020,02,11, 10, 45, 34), //Tuesday 
                Distance = 5,
                Time = 6,
                Latitude = 7.4,
                Longitude = 6.5,
            };
            
            var result = _mapper.Map<RunCreateRequest, RunDb>(createRunRequest);
            
            result.Date.Should().Be(createRunRequest.Date.Date);
            result.Distance.Should().Be(createRunRequest.Distance);
            result.Time.Should().Be(createRunRequest.Time);
            result.Latitude.Should().Be(createRunRequest.Latitude);
            result.Longitude.Should().Be(createRunRequest.Longitude);
        }

        
        [Fact]
        public void From_UpdateRunRequest_To_RunDb()
        {
            var updateRunRequest = new RunUpdateRequest()
            {
                Date = new DateTime(2020,02,16, 10, 10, 29), //Sunday 
                Distance = 5,
                Time = 6,
                Latitude = 7.4,
                Longitude = 6.5,
            };
            
            var result = _mapper.Map<RunUpdateRequest, RunDb>(updateRunRequest);
            
            result.Date.Should().Be(updateRunRequest.Date.Date);
            result.Distance.Should().Be(updateRunRequest.Distance);
            result.Time.Should().Be(updateRunRequest.Time);
            result.Latitude.Should().Be(updateRunRequest.Latitude);
            result.Longitude.Should().Be(updateRunRequest.Longitude);
        }
        [Fact]
        public void From_RunDb_To_RunDto()
        {
            var rundDb = new RunDb()
            {
                Id = 2,
                UserId = Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                Distance = 5,
                Time = 1000,
                Latitude = 78.8,
                Longitude = 45.45,
                WeatherConditions = "test weather conditions"
            };

            var runDto = _mapper.Map<RunDb, RunDto>(rundDb);

            runDto.Id.Should().Be(rundDb.Id);
            runDto.UserId.Should().Be(rundDb.UserId);
            runDto.Date.Should().Be(rundDb.Date);
            runDto.Distance.Should().Be(rundDb.Distance);
            runDto.Time.Should().Be(rundDb.Time);
            runDto.Latitude.Should().Be(rundDb.Latitude);
            runDto.Longitude.Should().Be(rundDb.Longitude);
            runDto.WeatherConditions.Should().Be(rundDb.WeatherConditions);
        }

        #endregion
    }
}