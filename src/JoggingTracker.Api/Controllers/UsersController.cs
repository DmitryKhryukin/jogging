using System.Threading.Tasks;
using JoggingTracker.Api.Attributes;
using JoggingTracker.Api.Helpers;
using JoggingTracker.Core;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JoggingTracker.Api.Controllers
{
    [Authorize]
    [ApiVersion("1")]
    [ApiController]
    [Route("api/v{version:apiVersion}/users/")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;

        public UsersController(ILogger<UsersController> logger,
            IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        #region registration

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateUserAsync([FromBody] UserRegisterRequest request)
        {
            var result = await _userService.CreateUserAsync(request);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        #endregion
        
        #region current user
        
        [Roles(UserRoles.Admin, UserRoles.UserManager, UserRoles.RegularUser)]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetCurrentUserAsync()
        {
            var currentUserId = HttpContext.GetCurrentUserId();
            
            var result = await _userService.GetCurrentUserAsync(currentUserId);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, ErrorMessages.UserNotFound);
            }
            
            return StatusCode(StatusCodes.Status200OK, result);
        }
        
        [Roles(UserRoles.Admin, UserRoles.UserManager, UserRoles.RegularUser)]
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateCurrentUserAsync(UserUpdateRequest request)
        {
            var currentUserId = HttpContext.GetCurrentUserId();
            
            var result = await _userService.UpdateUserAsync(currentUserId, request);

            return StatusCode(StatusCodes.Status200OK, result);
        }

        [Roles(UserRoles.Admin, UserRoles.UserManager, UserRoles.RegularUser)]
        [HttpDelete("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteCurrentUserAsync()
        {
            var currentUserId = HttpContext.GetCurrentUserId();
            
            await _userService.DeleteUserAsync(currentUserId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        
        #endregion
        
        #region user management

        [Roles(UserRoles.Admin, UserRoles.UserManager)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUsersAsync([FromQuery(Name = "$filter")] string filter, 
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize)
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();
            
            var result = await _userService.GetUsersAsync(isCurrentUserAdmin, filter, pageNumber, pageSize);

            return StatusCode(StatusCodes.Status200OK, result);
        }
        
        [Roles(UserRoles.Admin, UserRoles.UserManager)]
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUserWithRolesAsync(string userId)
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();
            
            var result = await _userService.GetUserWithRolesAsync(userId, isCurrentUserAdmin);
            
            if (result == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, ErrorMessages.UserNotFound);
            }

            return StatusCode(StatusCodes.Status200OK, result);
        }
        
        [Roles(UserRoles.Admin, UserRoles.UserManager)]
        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateUserAsync(string userId, UserWithRolesUpdateRequest request)
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();
            
            var result = await _userService.UpdateUserAsync(userId, request, isCurrentUserAdmin);

            return StatusCode(StatusCodes.Status200OK, result);
        }

        [Roles(UserRoles.Admin, UserRoles.UserManager)]
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteUserAsync(string userId)
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();
            
            await _userService.DeleteUserAsync(userId, isCurrentUserAdmin);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        #endregion
    }
}