using System;
using System.Linq.Expressions;
using FluentAssertions;
using JoggingTracker.Core.DTOs.Run;
using JoggingTracker.Core.DTOs.User;
using JoggingTracker.Core.Services;
using JoggingTracker.Core.Utils;
using JoggingTracker.DataAccess.DbEntities;
using Xunit;

namespace JoggingTracker.Core.Tests.Utils
{
    public class ExpressionConverterTests
    {
        [Fact]
        public void ConvertExpression_UserDto_To_UserDb()
        {
            // arrange
            Expression<Func<UserDto, bool>> userDtoPredicate = dto => (dto.UserName.Contains("a") && dto.UserName.StartsWith("b"))
                                                                      || dto.Id == "id";

            var expressionConverter = new ExpressionConverter<UserDto, UserDb>();

            // act
            var result = expressionConverter.Convert(userDtoPredicate);

            // assert
            Expression<Func<UserDb, bool>> expectedPredicate = p => (p.UserName.Contains("a") && p.UserName.StartsWith("b"))
                                                                    || p.Id == "id";
            
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedPredicate);
        }
        
        [Fact]
        public void ConvertExpression_RunDto_To_RunDb()
        {
            // arrange
            Expression<Func<RunDto, bool>> runDtoPredicate = dto => (dto.Distance > 4 && dto.Latitude < 10.10)
                                                                      || dto.Id == 5;

            var expressionConverter = new ExpressionConverter<RunDto, RunDb>();

            // act
            var result = expressionConverter.Convert(runDtoPredicate);

            // assert
            Expression<Func<RunDb, bool>> expectedPredicate = p => (p.Distance > 4 && p.Latitude < 10.10)
                                                                    || p.Id == 5;
            
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedPredicate);
        }
    }
}