using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using FluentAssertions;
using JoggingTracker.Core.DTOs;
using JoggingTracker.Core.DTOs.Run;
using JoggingTracker.Core.DTOs.User;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Xunit;
using Xunit.Priority;

namespace JoggingTracker.Api.Tests.IntegrationTests.EndToEndTests
{
    [Collection("Non-Parallel Collection")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [DefaultPriority(0)]
    public class RunsManagementTests : BaseIntegrationTest
    {
        private const string _baseUri = "/api/v1/users";
        private const string _baseRunsUri = "/api/v1/users/me/runs";
        
        public RunsManagementTests(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact, Priority(1)]
        public void UserGetsRegistered_AddsRuns_UpdatesRun_RequestList_And_DeletesTheirRecord_RunShouldBeDeleted()
        {
            // user gets registered 
            var newUserRequest = new UserRegisterRequest()
            {
                UserName = "newUserName",
                Password = "123456"
            };

            var httpCreateResponse = _client.PostAsync(_baseUri, ContentHelper.GetStringContent(newUserRequest)).Result;

            httpCreateResponse.EnsureSuccessStatusCode();

            var stringResponse = httpCreateResponse.Content.ReadAsStringAsync().Result;

            var user = JsonConvert.DeserializeObject<UserDto>(stringResponse);

            user.Id.Should().NotBeNull();

            var newUserDb = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            newUserDb.Should().NotBeNull();

            // user authenticates
            var token = GetUserToken(newUserRequest.UserName, newUserRequest.Password);

            token.Should().NotBeNull();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var newRun = new RunCreateRequest()
            {
                Date = new DateTime(2020, 2, 11),
                Distance = 500,
                Time = 500,
                Latitude = 2,
                Longitude = 2
            };
            
            var httpCreateRunResponse = _client.PostAsync(_baseRunsUri, ContentHelper.GetStringContent(newRun)).Result;

            httpCreateRunResponse.EnsureSuccessStatusCode();

            var createRunStringContent = httpCreateRunResponse.Content.ReadAsStringAsync().Result;
            
            var createRunDtoResponse = JsonConvert.DeserializeObject<RunDto>(createRunStringContent);

            createRunDtoResponse.UserId.Should().Be(newUserDb.Id);
            createRunDtoResponse.Should().NotBeNull();
            createRunDtoResponse.Date.Should().Be(newRun.Date);
            createRunDtoResponse.Distance.Should().Be(newRun.Distance);
            createRunDtoResponse.Time.Should().Be(newRun.Time);
            createRunDtoResponse.Latitude.Should().Be(newRun.Latitude);
            createRunDtoResponse.Longitude.Should().Be(newRun.Longitude);

            var dbRun = _dbContext.Runs.FirstOrDefault(x => x.Id == createRunDtoResponse.Id);
            dbRun.Should().NotBeNull();
            
            // update run
            var updateRunRequest = new RunUpdateRequest()
            {
                Date = new DateTime(2012, 2, 11),
                Distance = 30,
                Time = 12,
                Latitude = 54,
                Longitude = 45
            };
            
            var httpUpdateRunResponse = _client.PutAsync($"{_baseRunsUri}/{dbRun.Id}", ContentHelper.GetStringContent(updateRunRequest)).Result;

            httpUpdateRunResponse.EnsureSuccessStatusCode();

            var updateRunStringContent = httpUpdateRunResponse.Content.ReadAsStringAsync().Result;
            var updateRunDtoResponse = JsonConvert.DeserializeObject<RunDto>(updateRunStringContent);
            
            updateRunDtoResponse.UserId.Should().Be(newUserDb.Id);
            updateRunDtoResponse.Should().NotBeNull();
            updateRunDtoResponse.Date.Should().Be(updateRunRequest.Date);
            updateRunDtoResponse.Distance.Should().Be(updateRunRequest.Distance);
            updateRunDtoResponse.Time.Should().Be(updateRunRequest.Time);
            updateRunDtoResponse.Latitude.Should().Be(updateRunRequest.Latitude);
            updateRunDtoResponse.Longitude.Should().Be(updateRunRequest.Longitude);
            
            // get the list of runs
            var httpGetRunsListResponse = _client.GetAsync($"{_baseRunsUri}/").Result;

            httpGetRunsListResponse.EnsureSuccessStatusCode();
            var getRunsListStringContent = httpGetRunsListResponse.Content.ReadAsStringAsync().Result;
            var getRunsListDtoResponse = JsonConvert.DeserializeObject<PagedResult<RunDto>>(getRunsListStringContent);

            getRunsListDtoResponse.Items.Count.Should().Be(1);
            
            // user deletes their own record
            var httpDeleteResponse = _client.DeleteAsync($"{_baseUri}/me").Result;

            httpDeleteResponse.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            
            // user is not in the database anymore
            var deletedUser = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            deletedUser.Should().BeNull();
            
            var deletetRunDb = _dbContext.Runs.FirstOrDefault(x => x.Id == createRunDtoResponse.Id);
            deletetRunDb.Should().BeNull();
        }
    }
}