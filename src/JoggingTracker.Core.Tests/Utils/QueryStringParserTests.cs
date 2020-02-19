using System;
using System.Linq.Expressions;
using FluentAssertions;
using JoggingTracker.Core.DTOs.Run;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.Core.Utils;
using StringToExpression.Exceptions;
using Xunit;

namespace JoggingTracker.Core.Tests.Utils
{
    public class QueryStringParserTests
    {
        [Fact]
        public void ParseFilter_ForUserDto()
        {
            // arrange
            var filter = "((userName eq 'Test') or (userName eq '1')) and (id ne 'abc')";

            // act
            Expression<Func<UserDto, bool>> predicate = QueryStringParser.ParseFilter<UserDto>(filter);

            // assert
            predicate.Should().NotBeNull();
            predicate.Body.ToString().Should().BeEquivalentTo("(((Param_0.UserName == \"Test\") Or (Param_0.UserName == \"1\")) And (Param_0.Id != \"abc\"))");
        }
        
        [Fact]
        public void ParseFilter_ForUserDto_InvalidPropertyName_ThrowsException()
        {
            var filter = "((invalidName eq 'test') or (userName eq '1')) and id ne 'abc'";

            Assert.Throws<OperationInvalidException>(() => QueryStringParser.ParseFilter<UserDto>(filter)) ;
        }

        [Fact]
        public void ParseFilter_ForRunDto()
        {
            // arrange
            var filter = "((Date gt '2020-02-02') and (Distance lt 5)) OR (Latitude eq 5.5)";

            // act
            Expression<Func<RunDto, bool>> predicate = QueryStringParser.ParseFilter<RunDto>(filter);
            
            // assert
            predicate.Should().NotBeNull();
            predicate.Body.ToString().Should().BeEquivalentTo("(((Param_0.Date > 02/02/2020 00:00:00) And (Param_0.Distance < 5)) Or (Convert(Param_0.Latitude, Decimal) == 5.5))");
        }

        [Fact]
        public void FormatFilter_ToLowercase_ReplaceAllDateClausesCorrectly()
        {
            var filter = "(Date gt '2020-02-02') OR (Date lT '2020-02-02') OR (Date eQ '2020-02-02') OR (Date Ne '2020-02-02')";

            var result = QueryStringParser.FormatFilter(filter);

            result.Should().Be("(date gt datetime'2020-02-02') or (date lt datetime'2020-02-02') or (date eq datetime'2020-02-02') or (date ne datetime'2020-02-02')");
        }
    }
}