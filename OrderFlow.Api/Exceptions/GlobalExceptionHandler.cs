using Microsoft.AspNetCore.Diagnostics;

namespace OrderFlow.Api.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            string message;
            switch (exception)
            {
                case DomainException:
                    message = exception.Message;
                    httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                    break;

                case NotFoundException:
                    message = exception.Message;
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    break;

                default:
                    message = "An unexpected error occurred";
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    _logger.LogError(exception, "Unhandled exception occurred");
                    break;
            }

            await httpContext.Response.WriteAsJsonAsync(new
            {
                Message = message
            }, cancellationToken);

            return true;
        }
    }
}