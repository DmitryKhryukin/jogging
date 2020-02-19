using System.Net.Http;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.DataAccess;
using Newtonsoft.Json;
using Xunit;

namespace JoggingTracker.Api.Tests.IntegrationTests
{
    public class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        protected readonly HttpClient _client;
        protected readonly JoggingTrackerDataContext _dbContext;
        private static readonly string _authUri = "/api/v1/auth";

        public BaseIntegrationTest(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
            
            _dbContext = new JoggingTrackerDataContext(FakeDbUtilities.GetDbContextOptions());
        }

        protected string GetAdminAuthToken()
        {
            return GetUserToken(FakeDbUtilities.adminUser.UserName, FakeDbUtilities.UserPassword);
        }
        
        protected string GetUserManagerAuthToken()
        {
            return GetUserToken(FakeDbUtilities.managerUser.UserName, FakeDbUtilities.UserPassword);
        }
        
        protected string GetUserToken(string userName, string password)
        {
            var request = new UserLoginRequest()
            {
                UserName = userName,
                Password = password
            };
            
            var httpResponse = _client.PostAsync($"{_authUri}/token", ContentHelper.GetStringContent(request)).Result;

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;

            var response = JsonConvert.DeserializeObject<UserLoginResponse>(stringResponse);

            return response.Token;
        }
    }
}