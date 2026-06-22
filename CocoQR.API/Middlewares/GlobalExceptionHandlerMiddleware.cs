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
            catch (DomainException ex)
            {
                await HandleDomainExceptionAsync(context, ex);
            }
            catch (ApplicationException ex)
            {
                await HandleApplicationExceptionAsync(context, ex);
            }
            catch (ArgumentException ex)
            {
                await HandleArgumentExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                await HandleUnexpectedExceptionAsync(context, ex);
            }
        }

        private async Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
        {
            await HandleBusinessExceptionAsync(context, exception, ErrorCategory.DomainRuleViolation);
        }

        private async Task HandleApplicationExceptionAsync(HttpContext context, ApplicationException exception)
        {
            await HandleBusinessExceptionAsync(context, exception, ErrorCategory.ApplicationUseCaseFailure);
        }

        private async Task HandleArgumentExceptionAsync(HttpContext context, ArgumentException exception)
        {
            const int statusCode = StatusCodes.Status400BadRequest;

            _logger.LogWarning(exception,
                "Client error - Code: {Code}, Type: {Type}, Message: {Message}, User: {UserId}",
                ErrorCode.BadRequest,
                exception.GetType().Name,
                exception.Message,
                context.User?.FindFirst("id")?.Value ?? "anonymous");

            var data = _env.IsDevelopment() && !string.IsNullOrWhiteSpace(exception.ParamName)
                ? new { Parameter = exception.ParamName }
                : null;

            await WriteErrorResponseAsync(
                context,
                statusCode,
                ErrorCode.BadRequest,
                exception.Message,
                data,
                ErrorCategory.InvalidArgument);
        }

        private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception,
                "UNEXPECTED ERROR - Type: {Type}, Message: {Message}, Path: {Path}",
                exception.GetType().Name,
                exception.Message,
                context.Request.Path);

            var message = _env.IsDevelopment()
                ? exception.Message
                : ErrorMessages.UnexpectedError;

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
                ErrorCategory.SystemError);
        }

        private async Task HandleBusinessExceptionAsync<TException>(
            HttpContext context,
            TException exception,
            string category)
            where TException : Exception, IBusinessException
        {
            var statusCode = ExceptionStatusCodeMapper.MapToStatusCode(exception);
            LogByStatusCode(context, exception, statusCode, exception.Code, exception.Message);

            await WriteErrorResponseAsync(
                context,
                statusCode,
                exception.Code,
                exception.Message,
                exception.Data,
                category);
        }

        private void LogByStatusCode(
            HttpContext context,
            Exception exception,
            int statusCode,
            string code,
            string message)
        {
            if (ExceptionStatusCodeMapper.IsServerError(statusCode))
            {
                _logger.LogError(exception,
                    "Server error - Code: {Code}, Type: {Type}, Message: {Message}",
                    code,
                    exception.GetType().Name,
                    message);
                return;
            }

            if (ExceptionStatusCodeMapper.IsClientError(statusCode))
            {
                _logger.LogWarning(exception,
                    "Client error - Code: {Code}, Type: {Type}, Message: {Message}, User: {UserId}",
                    code,
                    exception.GetType().Name,
                    message,
                    context.User?.FindFirst("id")?.Value ?? "anonymous");
            }
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
