using GitHubIntegrationApi.Models;
using System.Net;
using System.Text.Json;

namespace GitHubIntegrationApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An internal server error occurred.";

            if (exception is UnauthorizedAccessException || exception.Message.Contains("401"))
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized access - check GitHub token or authentication.";
            }
            else if (exception is TaskCanceledException || exception is TimeoutException)
            {
                statusCode = HttpStatusCode.RequestTimeout;
                message = "The request timed out.";
            }

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.Fail(message);
            var result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return context.Response.WriteAsync(result);
        }
    }
}
