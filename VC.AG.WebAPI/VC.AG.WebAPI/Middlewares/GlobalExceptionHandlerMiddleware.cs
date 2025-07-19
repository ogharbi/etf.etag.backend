using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Mvc;
using VC.AG.Models;
using VC.AG.Models.Extensions;
using VC.AG.Models.Helpers;

namespace VC.AG.WebAPI.Middlewares
{
    public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
       
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                AppHelper.LogEntry(logger,$"Something went wrong: {ex}",AG.Models.Enums.LogType.Error);
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var traceId = Guid.NewGuid();
            AppHelper.LogEntry(logger, $"Error occure while processing the request, TraceId : ${traceId}, Message : ${ex.Message}, StackTrace: ${ex.StackTrace}", AG.Models.Enums.LogType.Error);
            var errorStatus = StatusCodes.Status500InternalServerError;
            if(ex.InnerException != null && ex.InnerException.Message.EqualsNotNull(AppConstants.Commun.UnauthorizedOp)) {
                 errorStatus = StatusCodes.Status403Forbidden;
            }
            context.Response.StatusCode = errorStatus;
            var problemDetails = new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = errorStatus,
                Instance = context.Request.Path,
                Detail = $"{ex.Message}. \n\nInternal server error occured, traceId : {traceId}",
            };
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}