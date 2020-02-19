using System;
using Microsoft.AspNetCore.Authorization;

namespace JoggingTracker.Api.Attributes
{
    public class RolesAttribute : AuthorizeAttribute
    {
        public RolesAttribute(params string[] roles)
        {
            Roles = String.Join(",", roles);
        }
    }
}