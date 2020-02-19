using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JoggingTracker.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace JoggingTracker.Core.Helpers
{
    public static class PaginationHelper
    {
        /// <summary>
        /// applies pagination to linq query
        /// </summary>
        /// <param name="query">linq query</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <typeparam name="TDto">DTO entity</typeparam>
        /// <typeparam name="TDb">Database entity</typeparam>
        /// <returns></returns>
        public static async Task<PagedResult<TDto>> GetPagedResponseAsync<TDb, TDto>(IQueryable<TDb> query,
            int? pageNumber, 
            int? pageSize,
            IMapper mapper) 
            where TDb : class
            where TDto: class
        {
            var result = new PagedResult<TDto>
            {
                PageNumber = pageNumber, 
                PageSize = pageSize
            };

            var count = await query.CountAsync();

            List<TDb> dbEntities;

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                var skip = (pageNumber.Value - 1) * pageSize.Value;

                dbEntities = await query.Skip(skip).Take(pageSize.Value).ToListAsync();
            }
            else
            {
                dbEntities = await query.ToListAsync();
            }

            if (dbEntities.Count > 0)
            {
                result.Items = dbEntities.Select(mapper.Map<TDb, TDto>).ToList();
            }

            result.Total = count;

            return result;
        }
        
        public static PagedResult<TDto> GetPagedResponse<TDto>(IEnumerable<TDto> allItems,
            int? pageNumber, 
            int? pageSize) 
            where TDto: class
        {
            var result = new PagedResult<TDto>
            {
                PageNumber = pageNumber, 
                PageSize = pageSize
            };

            var count = allItems.Count();
            
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                var skip = (pageNumber.Value - 1) * pageSize.Value;

                result.Items = allItems.Skip(skip).Take(pageSize.Value).ToList();
            }
            else
            {
                result.Items = allItems.ToList();
            }

            result.Total = count;

            return result;
        }
    }
}