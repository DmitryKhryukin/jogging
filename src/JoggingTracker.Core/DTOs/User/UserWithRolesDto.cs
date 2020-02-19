using System.Collections.Generic;

namespace JoggingTracker.Core.DTOs.User
{
    public class UserWithRolesDto : UserDto
    {
        public IEnumerable<string> Roles { get; set; }
    }
}