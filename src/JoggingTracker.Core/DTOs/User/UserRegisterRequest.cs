using System.ComponentModel.DataAnnotations;

namespace JoggingTracker.Core.DTOs.User
{
    public class UserRegisterRequest
    {
        [Required(ErrorMessage = ErrorMessages.UserNameIsRequired)]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = ErrorMessages.PasswordIsRequired)]
        [MinLength(Constants.AppConstants.MinPasswordLength, ErrorMessage = ErrorMessages.PasswordShouldBe6CharLong)]
        public string Password { get; set; }
    }
}