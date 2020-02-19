using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JoggingTracker.Core.DTOs.User
{
    public class UserWithRolesUpdateRequest : UserUpdateRequest
    {
        public IEnumerable<string> Roles { get; set; }
    }
}