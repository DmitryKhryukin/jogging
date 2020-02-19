using System.ComponentModel.DataAnnotations;

namespace JoggingTracker.Core.DTOs.User
{
    public class UserUpdateRequest
    {
        [Required(ErrorMessage = ErrorMessages.UserNameIsRequired)]
        public string UserName { get; set; }
        
        public string OldPassword { get; set; }
        
        public string NewPassword { get; set; }
    }
}