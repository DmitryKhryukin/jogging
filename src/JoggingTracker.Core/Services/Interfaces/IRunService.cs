using System.Collections.Generic;
using System.Threading.Tasks;
using JoggingTracker.Core.DTOs;
using JoggingTracker.Core.DTOs.Run;

namespace JoggingTracker.Core.Services.Interfaces
{
    public interface IRunService
    {
        Task<RunDto> CreateRunAsync(string userId, RunCreateRequest request);
        Task<PagedResult<RunDto>> GetRunsAsync(string userId, 
            string filter = null,
            int? pageNumber = null, 
            int? pageSize = null);
        Task<RunDto> GetRunAsync(string userId, int runId);
        Task<RunDto> UpdateRunAsync(string userId, int runId, RunUpdateRequest request);
        Task DeleteRunAsync(string userId, int runId);
        Task<PagedResult<RunsWeeksReport>> GetWeeksReportAsync(string userId,
            string filter = null,
            int? pageNumber = null,
            int? pageSize = null);
    }
}