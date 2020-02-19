using System.Net;
using FluentAssertions;
using JoggingTracker.Core;
using JoggingTracker.Core.DTOs.User;
using Newtonsoft.Json;
using Xunit;

namespace JoggingTracker.Api.Tests.IntegrationTests.Controllers
{
    public class AuthControllerTests : BaseIntegrationTest
    {
        private static readonly string _baseUri = "/api/v1/auth";
        private static readonly string _tokenUri = $"{_baseUri}/token";

        public AuthControllerTests(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public void GetToken_UserNameAndPasswordNotProvided_ReturnsBadRequest()
        {
            var request = new UserLoginRequest()
            {
                UserName = null,
                Password = null
            };
            var httpResponse = _client.PostAsync(_tokenUri, ContentHelper.GetStringContent(request)).Result;

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;

            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            stringResponse.Should().Contain(ErrorMessages.UserNameIsRequired);
            stringResponse.Should().Contain(ErrorMessages.PasswordIsRequired);
        }

        [Fact]
        public void GetToken_WrongCredentials_ReturnsUnauthorized()
        {
            var request = new UserLoginRequest()
            {
                UserName = "admin",
                Password = "wrongpassword"
            };
            var httpResponse = _client.PostAsync(_tokenUri, ContentHelper.GetStringContent(request)).Result;

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;

            httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void GetToken_CorrectCredentials_ReturnsToken()
        {
            var request = new UserLoginRequest()
            {
                UserName = FakeDbUtilities.SeedUsers[0].UserName,
                Password = FakeDbUtilities.UserPassword
            };
            var httpResponse = _client.PostAsync(_tokenUri, ContentHelper.GetStringContent(request)).Result;

            httpResponse.EnsureSuccessStatusCode();

            var stringResponse = httpResponse.Content.ReadAsStringAsync().Result;

            var response = JsonConvert.DeserializeObject<UserLoginResponse>(stringResponse);

            response.UserId.Should().Be(FakeDbUtilities.SeedUsers[0].Id);
            response.Token.Should().NotBeNullOrWhiteSpace();
        }
    }
}