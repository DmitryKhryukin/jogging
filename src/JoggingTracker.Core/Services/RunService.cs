using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JoggingTracker.Core.Constants;
using JoggingTracker.Core.DTOs;
using JoggingTracker.Core.DTOs.Run;
using JoggingTracker.Core.Exceptions;
using JoggingTracker.Core.Helpers;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.DataAccess;
using JoggingTracker.DataAccess.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace JoggingTracker.Core.Services
{
    public class RunService : IRunService
    {
        private readonly JoggingTrackerDataContext _dbContext;
        private readonly IWeatherService _weatherService;
        private readonly IMapper _mapper;
        
        public RunService(JoggingTrackerDataContext dbContext,
            IWeatherService weatherService,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _weatherService = weatherService;
            _mapper = mapper;
        }
        
        public async Task<RunDto> CreateRunAsync(string userId, RunCreateRequest request)
        {
            try
            {
                var weatherConditions = await _weatherService.GetWeatherConditionsAsync(request.Date, 
                    request.Latitude, 
                    request.Longitude);

                var runDb = _mapper.Map<RunCreateRequest, RunDb>(request);
                runDb.UserId = userId;
                runDb.WeatherConditions = weatherConditions;
                
                await _dbContext.AddAsync(runDb);
                await _dbContext.SaveChangesAsync();
                
                var response = _mapper.Map<RunDb, RunDto>(runDb);

                return response;
            }
            catch (Exception ex)
            {
                throw new JoggingTrackerInternalServerErrorException($"{ErrorMessages.RunSaveErrorMessage} : {ex.Message}");
            }
        }
        
        public async Task<RunDto> GetRunAsync(string userId, int runId)
        {
            RunDto result = null;
            
            var runDb = await _dbContext.Runs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId &&
                                          x.Id == runId);

            if (runDb != null)
            {
                result = _mapper.Map<RunDb, RunDto>(runDb);
            }

            return result;
        }

        /// <summary>
        /// a report on average speed & distance per week
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PagedResult<RunsWeeksReport>> GetWeeksReportAsync(string userId,
            string filter = null,
            int? pageNumber = null,
            int? pageSize = null)
        {
            var allUserRuns = await _dbContext.Runs.Where(x => x.UserId == userId).ToListAsync();

            var predicate = ExpressionHelper.GetFilterPredicate<RunsWeeksReport>(filter);

            var allReportItems = allUserRuns.GroupBy(x => x.Date.StartOfWeek(AppConstants.StartOfWeek))
                .OrderByDescending(x => x.Key)
                .Select(x =>
                    new RunsWeeksReport()
                    {
                        WeekStartDate = x.Key,
                        AverageDistance = Math.Round(x.Average(r => r.Distance), 2),
                        AverageSpeed = Math.Round(((double)x.Sum(r => r.Distance) / (double)x.Sum(r => r.Time)) * 3.6, 2),
                    })
                .Where(predicate.Compile())
                .OrderByDescending(x => x.WeekStartDate);

            return PaginationHelper.GetPagedResponse(allReportItems, pageNumber, pageSize);
        }

        public async Task<RunDto> UpdateRunAsync(string userId, 
            int runId, 
            RunUpdateRequest request)
        {
            try
            {
                // search by userId too to prevent using not related user id as a parameter
                var runDb = await _dbContext.Runs.FirstOrDefaultAsync(x => x.Id == runId
                                                                           && x.UserId == userId);
                if (runDb == null)
                {
                    throw new JoggingTrackerNotFoundException(ErrorMessages.RunNotFound);
                }
                
                if (IsLocationOrDateUpdated(runDb, request))
                {
                    var newWeatherConditions = await _weatherService.GetWeatherConditionsAsync(request.Date,
                        request.Latitude,
                        request.Longitude);

                    runDb.WeatherConditions = newWeatherConditions;
                }

                _mapper.Map(request, runDb);

                _dbContext.Update(runDb);
                await _dbContext.SaveChangesAsync();

                var response = _mapper.Map<RunDb, RunDto>(runDb);

                return response;
            }
            catch (JoggingTrackerNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new JoggingTrackerInternalServerErrorException(
                    $"{ErrorMessages.RunSaveErrorMessage} : {ex.Message}");
            }
        }
        
        public async Task DeleteRunAsync(string userId, int runId)
        {
            try
            {
                var runDb = await _dbContext.Runs.FirstOrDefaultAsync(x => x.UserId == userId &&
                                                                           x.Id == runId);
                if (runDb == null)
                {
                    throw new JoggingTrackerNotFoundException(ErrorMessages.RunNotFound);
                }
                
                _dbContext.Remove(runDb);
                await _dbContext.SaveChangesAsync();
            }
            catch (JoggingTrackerNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new JoggingTrackerInternalServerErrorException(
                    $"{ErrorMessages.RunDeleteErrorMessage} : {ex.Message}");
            }
        }
        
        public async Task<PagedResult<RunDto>> GetRunsAsync(string userId, 
            string filter = null, 
            int? pageNumber = null, 
            int? pageSize = null)
        {
            var predicate = ExpressionHelper.GetFilterPredicate<RunDb, RunDto>(filter);
            
            var query = _dbContext.Runs
                .AsNoTracking()
                .Where(predicate)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Date);

            var result = await PaginationHelper.GetPagedResponseAsync<RunDb, RunDto>(query, pageNumber, pageSize, _mapper);

            return result;
        }
        
        protected bool IsLocationOrDateUpdated(RunDb runDb, RunUpdateRequest request)
        {
            return runDb.Date != request.Date ||
                   Math.Abs(runDb.Latitude - request.Latitude) >= Constants.AppConstants.LocationTolerance||
                   Math.Abs(runDb.Longitude - request.Longitude) > Constants.AppConstants.LocationTolerance;
        }
    }
}