using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JoggingTracker.Core;
using JoggingTracker.Core.DTOs;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.DataAccess;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Xunit;
using Xunit.Priority;

namespace JoggingTracker.Api.Tests.IntegrationTests.Controllers
{
    [Collection("Non-Parallel Collection")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [DefaultPriority(0)]
    public class UsersControllerTests : BaseIntegrationTest
    {
        private const string _baseUri = "/api/v1/users";

        public UsersControllerTests(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact, Priority(1)]
        public void GetUsers_NoFilters_AdminRequests_ReturnsAllUsers()
        {
            var seedUsers = FakeDbUtilities.SeedUsers;

            var token = GetAdminAuthToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpResponse = _client.GetAsync(_baseUri).Result;

            httpResponse.EnsureSuccessStatusCode();

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;
            var users = JsonConvert.DeserializeObject<PagedResult<UserDto>>(stringResponse);

            users.Should().NotBeNull();
            users.Items.Count.Should().Be(seedUsers.Count);
            users.Total.Should().Be(seedUsers.Count);
        }
        
        [Fact, Priority(1)]
        public void GetUsers_NoFilters_UserManagerRequests_ReturnsAllUsersButAdmin()
        {
            var token = GetUserManagerAuthToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpResponse = _client.GetAsync(_baseUri).Result;

            httpResponse.EnsureSuccessStatusCode();

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;
            var users = JsonConvert.DeserializeObject<PagedResult<UserDto>>(stringResponse);
            
            var expectedUsers = new[] {FakeDbUtilities.regularUser, FakeDbUtilities.managerUser};

            users.Should().NotBeNull();
            users.Items.Count.Should().Be(expectedUsers.Length);
            users.Total.Should().Be(expectedUsers.Length);

            users.Items.Should().ContainSingle(x => x.Id == expectedUsers[0].Id);
            users.Items.Should().ContainSingle(x => x.Id == expectedUsers[1].Id);
        }
        
        [Fact, Priority(2)]
        public void GetUsers_WithFilter_ReturnsUsersBasedOnFilter()
        {
            var seedUsers = FakeDbUtilities.SeedUsers;

            var filter = "(userName ne 'testUserName1Admin') and (userName ne 'testUserName2Usermanager')";

            var token = GetAdminAuthToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpResponse = _client.GetAsync($"{_baseUri}?$filter={filter}").Result;

            httpResponse.EnsureSuccessStatusCode();

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;
            var users = JsonConvert.DeserializeObject<PagedResult<UserDto>>(stringResponse);

            users.Should().NotBeNull();
            users.Items.Count.Should().Be(1);
            users.Total.Should().Be(1);
            users.Items.ElementAt(0).UserName.Should().BeEquivalentTo("testUserName3Regular");
        }

        [Fact, Priority(3)]
        public void RegisterUser_UserNameAndPasswordNotProvided_ReturnsBadRequest()
        {
            var request = new UserRegisterRequest()
            {
                UserName = null,
                Password = null
            };
            var httpResponse = _client.PostAsync(_baseUri, ContentHelper.GetStringContent(request)).Result;
            
            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;

            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            stringResponse.Should().Contain(ErrorMessages.UserNameIsRequired);
            stringResponse.Should().Contain(ErrorMessages.PasswordIsRequired);
        }

        [Fact, Priority(3)]
        public void RegisterUser_NullRequest_ReturnsBadRequest()
        {
            var httpResponse = _client.PostAsync(_baseUri, ContentHelper.GetStringContent(null)).Result;
            
            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        
        [Fact, Priority(4)]
        public void CreateUser_PasswordIsShort_ReturnsBadRequest()
        {
            var request = new UserRegisterRequest()
            {
                UserName = "username",
                Password = "123"
            };
            var httpResponse = _client.PostAsync(_baseUri, ContentHelper.GetStringContent(request)).Result;
            
            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;

            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            stringResponse.Should().Contain(ErrorMessages.PasswordShouldBe6CharLong);
        }
        
        [Fact, Priority(5)]
        public void CreateUser_ValidInput_NewUserIsRegistered() // TODO: check new user role
        {
            var request = new UserRegisterRequest()
            {
                UserName = "newRegisteredUser",
                Password = "123456"
            };
            var httpResponse = _client.PostAsync(_baseUri, ContentHelper.GetStringContent(request)).Result;
            
            httpResponse.EnsureSuccessStatusCode();
            
            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;
            
            var user = JsonConvert.DeserializeObject<UserDto>(stringResponse);
            user.Id.Should().NotBeNull();
            user.UserName.Should().Be(request.UserName);

            var seedUserIds = FakeDbUtilities.SeedUsers.Select(x => x.Id).ToList();
            var newUser = _dbContext.Users.FirstOrDefault(x => !seedUserIds.Contains(x.Id));
            
            newUser.Should().NotBeNull();
            newUser.UserName.Should().Be(request.UserName);
        }
        
        [Fact, Priority(6)]
        public void UpdateUser_AdminUserIsUpdated_UserMangerRequests_ThrowException()
        {
            var seedUsers = FakeDbUtilities.SeedUsers;

            var token = GetUserManagerAuthToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            UserWithRolesUpdateRequest request = new UserWithRolesUpdateRequest()
            {
                UserName = "test"
            };
            
            var httpResponse = _client.PutAsync($"{_baseUri}/{FakeDbUtilities.adminUser.Id}", ContentHelper.GetStringContent(request)).Result;

            httpResponse.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }
        
        [Fact, Priority(7)]
        public void UpdateUser_PromotesThemselvesToAdmin_ThrowException()
        {
            var seedUsers = FakeDbUtilities.SeedUsers;

            var token = GetUserManagerAuthToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            UserWithRolesUpdateRequest request = new UserWithRolesUpdateRequest()
            {
                UserName = "test",
                Roles = new []{ UserRoles.Admin }
            };
            
            var httpResponse = _client.PutAsync($"{_baseUri}/{FakeDbUtilities.managerUser.Id}", ContentHelper.GetStringContent(request)).Result;

            httpResponse.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }
        
        [Fact, Priority(8)]
        public void UpdateUser_PromotesRegularUserToUserManager_ReturnsOk()
        {
            var seedUsers = FakeDbUtilities.SeedUsers;

            var token = GetUserManagerAuthToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            UserWithRolesUpdateRequest request = new UserWithRolesUpdateRequest()
            {
                UserName = FakeDbUtilities.regularUser.UserName,
                Roles = new []{ UserRoles.RegularUser, UserRoles.UserManager }
            };
            
            var httpResponse = _client.PutAsync($"{_baseUri}/{FakeDbUtilities.regularUser.Id}", ContentHelper.GetStringContent(request)).Result;

            httpResponse.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}