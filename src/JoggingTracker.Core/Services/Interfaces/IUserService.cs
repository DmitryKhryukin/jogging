using System.Collections.Generic;
using System.Threading.Tasks;
using JoggingTracker.Core.DTOs;
using JoggingTracker.Core.DTOs.User;

namespace JoggingTracker.Core.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request);
        Task<UserDto> CreateUserAsync(UserRegisterRequest request);
        Task<UserDto> GetCurrentUserAsync(string userId);
        Task<UserWithRolesDto> GetUserWithRolesAsync(string userId, bool isCurrentUserAdmin);
        Task<PagedResult<UserDto>> GetUsersAsync(bool isCurrentUserAdmin, string filter = null, int? pageNumber = null, int? pageSize = null);
        Task<UserWithRolesDto> UpdateUserAsync(string userId, UserWithRolesUpdateRequest request, bool isCurrentUserAdmin);
        Task<UserDto> UpdateUserAsync(string userId, UserUpdateRequest request, IEnumerable<string> userRoles = null, bool? isCurrentUserAdmin = null);
        Task DeleteUserAsync(string userId, bool? isCurrentUserAdmin = null);
    }
}