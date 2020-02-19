using System.Threading.Tasks;
using JoggingTracker.Api.Attributes;
using JoggingTracker.Api.Helpers;
using JoggingTracker.Core;
using JoggingTracker.Core.DTOs.Run;
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
    public class RunsController : ControllerBase
    {
        private readonly ILogger<RunsController> _logger;
        private readonly IRunService _runService;

        public RunsController(ILogger<RunsController> logger,
            IRunService runService)
        {
            _logger = logger;
            _runService = runService;
        }
        
          #region current user

        [Roles(UserRoles.RegularUser, UserRoles.Admin)]
        [Route("me/runs")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateCurrentUserRun([FromBody] RunCreateRequest request)
        {
            var currentUserId = HttpContext.GetCurrentUserId();

            var response = await _runService.CreateRunAsync(currentUserId, request);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        
        [Roles(UserRoles.RegularUser, UserRoles.Admin)]
        [HttpGet("me/runs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetCurentUserRunsAsync([FromQuery(Name = "$filter")] string filter,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize)
        {
            var currentUserId = HttpContext.GetCurrentUserId();
            
            var response = await _runService.GetRunsAsync(currentUserId, filter, pageNumber, pageSize);

            return StatusCode(StatusCodes.Status200OK, response);
        }
        
        [Roles(UserRoles.RegularUser, UserRoles.Admin)]
        [HttpGet("me/runs/{runId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetCurrentUserRun(int runId)
        {
            var currentUserId = HttpContext.GetCurrentUserId();
            
            var run = await _runService.GetRunAsync(currentUserId, runId);

            if (run == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, ErrorMessages.RunNotFound);
            }
            
            return StatusCode(StatusCodes.Status200OK, run);
        }
        
        [Roles(UserRoles.RegularUser, UserRoles.Admin)]
        [HttpPut("me/runs/{runId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateCurrentUserRunAsync(int runId, [FromBody] RunUpdateRequest request)
        {
            var currentUserId = HttpContext.GetCurrentUserId();
            
            var result = await _runService.UpdateRunAsync(currentUserId, runId, request);

            return StatusCode(StatusCodes.Status200OK, result);
        }
        
        [Roles(UserRoles.RegularUser, UserRoles.Admin)]
        [HttpDelete("me/runs/{runId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteCurrentUserRunAsync(int runId)
        {
            var currentUserId = HttpContext.GetCurrentUserId();
            
            await _runService.DeleteRunAsync(currentUserId, runId);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [Roles(UserRoles.RegularUser, UserRoles.Admin)]
        [HttpGet("me/runs/report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetCurrentUserWeekRunsReportAsync([FromQuery(Name = "$filter")] string filter, 
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize)
        {
            var currentUserId = HttpContext.GetCurrentUserId();
            
            var response = await _runService.GetWeeksReportAsync(currentUserId, filter, pageNumber, pageSize);

            return StatusCode(StatusCodes.Status200OK, response);
        }
        
        #endregion

        #region admin management
        
        [Roles(UserRoles.Admin)]
        [HttpPost("{userId}/runs")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateRun(string userId, [FromBody] RunCreateRequest request)
        {
            var response = await _runService.CreateRunAsync(userId, request);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        
        [Roles(UserRoles.Admin)]
        [HttpGet("{userId}/runs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetRunsAsync(string userId,
            [FromQuery(Name = "$filter")] string filter,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize)
        {
            var response = await _runService.GetRunsAsync(userId, filter, pageNumber, pageSize);

            return StatusCode(StatusCodes.Status200OK, response);
        }
        
        [Roles(UserRoles.Admin)]
        [HttpGet("{userId}/runs/{runId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUserRun(string userId, int runId)
        {
            var run = await _runService.GetRunAsync(userId, runId);

            if (run == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, ErrorMessages.RunNotFound);
            }
            
            return StatusCode(StatusCodes.Status200OK, run);
        }
        
        [Roles(UserRoles.Admin)]
        [HttpPut("{userId}/runs/{runId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateRunAsync(string userId, int runId, [FromBody] RunUpdateRequest request)
        {
            var result = await _runService.UpdateRunAsync(userId, runId, request);

            return StatusCode(StatusCodes.Status200OK, result);
        }
        
        [Roles(UserRoles.Admin)]
        [HttpDelete("{userId}/runs/{runId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteRunAsync(string userId, int runId)
        {
            await _runService.DeleteRunAsync(userId, runId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        
        [Roles(UserRoles.Admin)]
        [HttpGet("{userId}/runs/report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUserWeekRunsReportAsync(string userId,
            [FromQuery(Name = "$filter")] string filter, 
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
        {
            var response = await _runService.GetWeeksReportAsync(userId, filter, pageNumber, pageSize);

            return StatusCode(StatusCodes.Status200OK, response);
        }
        
        #endregion
    }
}