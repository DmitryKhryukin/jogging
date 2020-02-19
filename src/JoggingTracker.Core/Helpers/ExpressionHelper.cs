using System;
using System.Linq.Expressions;
using JoggingTracker.Core.Exceptions;
using JoggingTracker.Core.Utils;

namespace JoggingTracker.Core.Helpers
{
    public static class ExpressionHelper 
    {
        /// <summary>
        /// Parses string filter to DTO entity predicate and converts it to DB entity predicate
        /// </summary>
        /// <param name="filter">string filter</param>
        /// <typeparam name="TDb">DB entity</typeparam>
        /// <typeparam name="TDto">DTO entity</typeparam>
        /// <returns></returns>
        /// <exception cref="JoggingTrackerBadRequestException"></exception>
        public static Expression<Func<TDb, bool>> GetFilterPredicate<TDb, TDto>(string filter = null)
            where TDb: class
            where TDto: class
        {
            Expression<Func<TDb, bool>> result = x => true;
            
            if (!string.IsNullOrWhiteSpace(filter))
            {
                try
                {
                    var dtoPredicate = QueryStringParser.ParseFilter<TDto>(filter);
                    
                    var expressionConverter = new ExpressionConverter<TDto, TDb>();

                    result = expressionConverter.Convert(dtoPredicate);
                }
                catch(Exception e)
                {
                    throw new JoggingTrackerBadRequestException($"{ErrorMessages.CouldntParseFilter} {e.Message}");
                }
            }

            return result;
        }
        
        /// <summary>
        /// parses string filter to DTO predicate
        /// </summary>
        /// <param name="filter">string filter</param>
        /// <typeparam name="TDto">DTO entity</typeparam>
        /// <returns></returns>
        /// <exception cref="JoggingTrackerBadRequestException"></exception>
        public static Expression<Func<TDto, bool>> GetFilterPredicate<TDto>(string filter = null) where TDto: class
        {
            Expression<Func<TDto, bool>> result = x => true;
            
            if (!string.IsNullOrWhiteSpace(filter))
            {
                try
                {
                   result = QueryStringParser.ParseFilter<TDto>(filter);
                }
                catch(Exception e)
                {
                    throw new JoggingTrackerBadRequestException($"{ErrorMessages.CouldntParseFilter} {e.Message}");
                }
            }

            return result;
        }
    }
}