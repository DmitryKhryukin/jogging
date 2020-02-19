using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JoggingTracker.DataAccess;
using Microsoft.AspNetCore.Http;

namespace JoggingTracker.Api.Helpers
{
    public static class HttpContextHelper
    {
        public static string GetCurrentUserId(this HttpContext httpContext)
        {
            return httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
        
        public static bool IsCurrentUserAdmin(this HttpContext httpContext)
        {
            return httpContext.User.IsInRole(UserRoles.Admin);
        }
    }
}