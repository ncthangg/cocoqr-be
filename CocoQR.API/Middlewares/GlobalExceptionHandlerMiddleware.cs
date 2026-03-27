using CocoQR.API.Mapper;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;


namespace CocoQR.API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)  // ← Domain rule violations
            {
                await HandleDomainExceptionAsync(context, ex);
            }
            catch (ApplicationException ex)  // ← Use case failures
            {
                await HandleApplicationExceptionAsync(context, ex);
            }
            catch (Exception ex)  // ← Unexpected system errors
            {
                await HandleUnexpectedExceptionAsync(context, ex);
            }
        }

        private async Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
        {
            // Use mapper to get status code
            var statusCode = ExceptionStatusCodeMapper.MapToStatusCode(exception);

            // Log based on severity
            if (ExceptionStatusCodeMapper.IsServerError(statusCode))
            {
                // 5xx errors - Log as ERROR (nghiêm trọng)
                _logger.LogError(exception,
                    "Server error - Code: {Code}, Type: {Type}, Message: {Message}",
                    exception.Code,
                    exception.GetType().Name,  // ← Log exception type
                    exception.Message);
            }
            else if (ExceptionStatusCodeMapper.IsClientError(statusCode))
            {
                // 4xx errors - Log as WARNING (không nghiêm trọng)
                _logger.LogWarning(
                    "Client error - Code: {Code}, Type: {Type}, Message: {Message}, User: {UserId}",
                    exception.Code,
                    exception.GetType().Name,
                    exception.Message,
                    context.User?.FindFirst("id")?.Value ?? "anonymous");
            }

            await WriteErrorResponseAsync(context, statusCode, exception.Code, exception.Message, exception.Data, "Domain Rule Violation");
        }
        private async Task HandleApplicationExceptionAsync(HttpContext context, ApplicationException exception)
        {
            // Application exceptions have varied status codes
            var statusCode = ExceptionStatusCodeMapper.MapToStatusCode(exception);

            if (ExceptionStatusCodeMapper.IsServerError(statusCode))
            {
                _logger.LogError(exception,
                    "Server error - Code: {Code}, Type: {Type}, Message: {Message}",
                    exception.Code,
                    exception.GetType().Name,  // ← Log exception type
                    exception.Message);
            }
            else if (ExceptionStatusCodeMapper.IsClientError(statusCode))
            {
                _logger.LogWarning(
                    "Client error - Code: {Code}, Type: {Type}, Message: {Message}, User: {UserId}",
                    exception.Code,
                    exception.GetType().Name,
                    exception.Message,
                    context.User?.FindFirst("id")?.Value ?? "anonymous");
            }

            await WriteErrorResponseAsync(context, statusCode, exception.Code, exception.Message, exception.Data, "Application Use Case Failure");
        }
        private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception exception)
        {
            // Log UNEXPECTED errors as ERROR
            _logger.LogError(exception,
                "UNEXPECTED ERROR - Type: {Type}, Message: {Message}, Path: {Path}",
                exception.GetType().Name,
                exception.Message,
                context.Request.Path);

            var message = _env.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred. Please try again later.";

            var data = _env.IsDevelopment()
                ? new
                {
                    Type = exception.GetType().Name,
                    StackTrace = exception.StackTrace,
                    InnerException = exception.InnerException?.Message
                }
                : null;

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                ErrorCode.InternalError,
                message,
                data,
                "System Error");
        }

        private static async Task WriteErrorResponseAsync(
            HttpContext context,
            int statusCode,
            string code,
            string message,
            object? data,
            string category)
        {
            if (context.Response.HasStarted) return;

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Code = code,
                Message = message,
                Data = data,
                Category = category,
                StatusCode = statusCode,
                StatusDescription = ExceptionStatusCodeMapper.GetStatusDescription(statusCode),
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier,
                Path = context.Request.Path
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };

            await context.Response.WriteAsJsonAsync(response, options);
        }
    }

    /// <summary>
    /// Standardized error response model
    /// </summary>
    public class ErrorResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string Category { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }
}
