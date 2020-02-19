using System;
using System.Threading.Tasks;
using JoggingTracker.Core;
using JoggingTracker.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JoggingTracker.Api.ErrorHandling
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionMiddleware> _logger;
        private readonly bool _isDevelopment;

        public CustomExceptionMiddleware(RequestDelegate next, 
            ILogger<CustomExceptionMiddleware> logger,
            bool isDevelopment)
        {
            _next = next;
            _logger = logger;
            _isDevelopment = isDevelopment;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var responseStatusCode = StatusCodes.Status500InternalServerError;
            var message = "";
            var logLevel = LogLevel.Warning;

            if (exception is JoggingTrackerNotFoundException)
            {
                responseStatusCode = StatusCodes.Status404NotFound;
                message = exception.Message;
            }
            else if (exception is JoggingTrackerBadRequestException)
            {
                responseStatusCode = StatusCodes.Status400BadRequest;
                message = exception.Message;
            }
            else if (exception is JoggingTrackerForbiddenException)
            {
                responseStatusCode = StatusCodes.Status403Forbidden;
                message = exception.Message;
            }
            else
            {
                logLevel = LogLevel.Error;
                message = _isDevelopment ? exception.Message 
                                         : ErrorMessages.InternalServerError;
            }
            
            _logger.Log(logLevel, exception.Message);
            
            var result = new ErrorDetails()
            {
                StatusCode = responseStatusCode,
                Message = message
            }.ToString();

            context.Response.StatusCode = responseStatusCode;
            return context.Response.WriteAsync(result);
        }
    }
}