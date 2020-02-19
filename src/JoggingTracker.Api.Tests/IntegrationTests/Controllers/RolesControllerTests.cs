using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using FluentAssertions;
using JoggingTracker.Core.DTOs;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.DataAccess;
using Newtonsoft.Json;
using Xunit;
using Xunit.Priority;

namespace JoggingTracker.Api.Tests.IntegrationTests.Controllers
{
    [Collection("Non-Parallel Collection")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [DefaultPriority(0)]
    public class RolesControllerTests : BaseIntegrationTest
    {
        private const string _baseUri = "/api/v1/users/roles/";
        
        public RolesControllerTests(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }
        
        [Fact, Priority(1)]
        public void GetRoles_AdminRequests_ReturnsAllRoles()
        {
            var seedUsers = FakeDbUtilities.SeedUsers;

            var token = GetAdminAuthToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpResponse = _client.GetAsync(_baseUri).Result;

            httpResponse.EnsureSuccessStatusCode();

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;
            var roles = JsonConvert.DeserializeObject<List<string>>(stringResponse);

            roles.Should().NotBeNull();
            roles.Count.Should().Be(3);
            roles.Should().BeEquivalentTo(UserRoles.AllRoles);
        }
        
        [Fact, Priority(1)]
        public void GetRoles_UserManagerRequests_ReturnsAllRolesButAdmin()
        {
            var expectedRoles = new [] { UserRoles.RegularUser, UserRoles.UserManager};
            var token = GetUserManagerAuthToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpResponse = _client.GetAsync(_baseUri).Result;

            httpResponse.EnsureSuccessStatusCode();

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;
            var roles = JsonConvert.DeserializeObject<List<string>>(stringResponse);

            roles.Should().NotBeNull();
            roles.Count.Should().Be(2);
            roles.Should().BeEquivalentTo(expectedRoles);
        }
    }
}