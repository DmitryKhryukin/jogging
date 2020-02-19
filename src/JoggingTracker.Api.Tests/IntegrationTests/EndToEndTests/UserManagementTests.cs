using System.Linq;
using System.Net.Http.Headers;
using FluentAssertions;
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
    public class UserManagementTests : BaseIntegrationTest
    {
        private const string _baseUri = "/api/v1/users";
        
        public UserManagementTests(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact, Priority(1)]
        public void UserGetsRegistered_And_DeletesTheirRecord()
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

            // user deletes their own record
            var httpDeleteResponse = _client.DeleteAsync($"{_baseUri}/me").Result;

            httpDeleteResponse.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            
            // user is not in the database anymore
            var deletedUser = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            deletedUser.Should().BeNull();
        }
        
        [Fact, Priority(2)]
        public void UserGetsRegistered_ChangesUserName_GetsTheirRecord_DeletesTheirRecord()
        {
            var baseMeUri = $"{_baseUri}/me";
            
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

            // user changes user name

            var updatedUserName = "updatedUserName";
            
            var updateRequest = new UserUpdateRequest()
            {
                UserName = updatedUserName
            };
            var httpUpdateResponse = _client.PutAsync(baseMeUri, ContentHelper.GetStringContent(updateRequest)).Result;
            httpUpdateResponse.StatusCode.Should().Be(StatusCodes.Status200OK);
            
            // gets info
            var httpInfoResponse = _client.GetAsync(baseMeUri).Result;

            // old token is not valid anymore
            httpInfoResponse.StatusCode.Should().Be(StatusCodes.Status200OK);
            
            var userInfo = JsonConvert.DeserializeObject<UserDto>(httpInfoResponse.Content.ReadAsStringAsync().Result);
            userInfo.UserName.Should().Be(updatedUserName);

            // user deletes their own record
            var httpDeleteResponse = _client.DeleteAsync(baseMeUri).Result;

            httpDeleteResponse.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            
            // user is not in the database anymore
            var deletedUser = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            deletedUser.Should().BeNull();
        }
    }
}