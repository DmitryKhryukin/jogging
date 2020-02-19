using System.Net.Http.Headers;
using FluentAssertions;
using JoggingTracker.Core.DTOs;
using JoggingTracker.Core.DTOs.Run;
using Newtonsoft.Json;
using Xunit;
using Xunit.Priority;

namespace JoggingTracker.Api.Tests.IntegrationTests.Controllers
{
    [Collection("Non-Parallel Collection")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [DefaultPriority(0)]
    public class RunsControllerTests : BaseIntegrationTest
    {
        private const string _baseUriFormat = "api/v1/users/{0}/runs";

        public RunsControllerTests(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }
        
        [Fact, Priority(1)]
        public void GetRuns_NoFilters_ReturnsAllUserRuns()
        {
            var seedRuns = FakeDbUtilities.SeedRegularUserRuns;

            var token = GetUserToken(FakeDbUtilities.adminUser.UserName, FakeDbUtilities.UserPassword);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var uri = string.Format(_baseUriFormat, FakeDbUtilities.regularUser.Id);
            
            var httpResponse = _client.GetAsync(uri).Result;

            httpResponse.EnsureSuccessStatusCode();

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;
            var users = JsonConvert.DeserializeObject<PagedResult<RunDto>>(stringResponse);

            users.Should().NotBeNull();
            users.Items.Count.Should().Be(seedRuns.Count);
            users.Total.Should().Be(seedRuns.Count);
        }
    }
}