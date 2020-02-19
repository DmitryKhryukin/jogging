using System;
using System.Globalization;
using System.Linq.Expressions;
using StringToExpression.LanguageDefinitions;

namespace JoggingTracker.Core.Utils
{
    public static class QueryStringParser
    {
        public static Expression<Func<T, bool>>  ParseFilter<T>(string filter) where T: class
        {
            var language = new ODataFilterLanguage();

            var formatterFilter = FormatFilter(filter);
            
            return language.Parse<T>(formatterFilter);
        }

        public static string FormatFilter(string filter)
       {
           var result = filter;
           
           if (!string.IsNullOrWhiteSpace(filter))
           {
               result = filter
                   .Replace(" EQ ", " eq ", true, CultureInfo.InvariantCulture)
                   .Replace(" NE ", " ne ", true, CultureInfo.InvariantCulture)
                   .Replace(" LT ", " lt ", true, CultureInfo.InvariantCulture)
                   .Replace(" GT ", " gt ", true, CultureInfo.InvariantCulture)
                   .Replace(" AND ", " and ", true, CultureInfo.InvariantCulture)
                   .Replace(" OR ", " or ", true, CultureInfo.InvariantCulture)
                   .Replace("date eq '", "date eq datetime'", true, CultureInfo.InvariantCulture)
                   .Replace("date ne '", "date ne datetime'", true, CultureInfo.InvariantCulture)
                   .Replace("date lt '", "date lt datetime'", true, CultureInfo.InvariantCulture)
                   .Replace("date gt '", "date gt datetime'", true, CultureInfo.InvariantCulture);
           }

           return result;
       }
    }
}