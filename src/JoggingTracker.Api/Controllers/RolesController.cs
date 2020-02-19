using System.Threading.Tasks;
using JoggingTracker.Api.Attributes;
using JoggingTracker.Api.Helpers;
using JoggingTracker.Core;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoggingTracker.Api.Controllers
{
    [Authorize]
    [ApiVersion("1")]
    [ApiController]
    [Route("api/v{version:apiVersion}/users/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        
        [Roles(UserRoles.Admin, UserRoles.UserManager)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetCurrentUserAsync()
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();

            var result = await _roleService.GetRolesAsync(isCurrentUserAdmin);
            
            return StatusCode(StatusCodes.Status200OK, result);
        }
    }
}