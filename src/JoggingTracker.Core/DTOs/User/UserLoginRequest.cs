using System.ComponentModel.DataAnnotations;

namespace JoggingTracker.Core.DTOs.User
{
    public class UserLoginRequest
    {
        [Required(ErrorMessage = ErrorMessages.UserNameIsRequired)]
        public string UserName { get; set; }
        [Required(ErrorMessage = ErrorMessages.PasswordIsRequired)]
        public string Password { get; set; }
    }
}