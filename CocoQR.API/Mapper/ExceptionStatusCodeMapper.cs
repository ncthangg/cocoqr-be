using CocoQR.Domain.Constants;
using CocoQR.Domain.Exceptions;

namespace CocoQR.API.Mapper
{
    public static class ExceptionStatusCodeMapper
    {
        private static readonly Dictionary<string, int> _errorCodeToHttpStatus = new()
        {
            // Client Errors (4xx)
            { ErrorCode.BadRequest, StatusCodes.Status400BadRequest },
            { ErrorCode.Unauthorized, StatusCodes.Status401Unauthorized },
            { ErrorCode.Forbidden, StatusCodes.Status403Forbidden },
            { ErrorCode.NotFound, StatusCodes.Status404NotFound },
            { ErrorCode.Conflict, StatusCodes.Status409Conflict },
            { ErrorCode.ValidationError, StatusCodes.Status422UnprocessableEntity },
            { ErrorCode.UnprocessableEntity, StatusCodes.Status422UnprocessableEntity },
            
            // Domain-specific errors (400)
            { ErrorCode.InvalidEmail, StatusCodes.Status400BadRequest },
            { ErrorCode.InvalidProjectState, StatusCodes.Status400BadRequest },
            { ErrorCode.BusinessRuleViolation, StatusCodes.Status400BadRequest },
            { ErrorCode.DuplicateEntry, StatusCodes.Status409Conflict },
            
            // Server Errors (5xx)
            { ErrorCode.InternalError, StatusCodes.Status500InternalServerError },
            { ErrorCode.ServiceUnavailable, StatusCodes.Status503ServiceUnavailable },
            { ErrorCode.DatabaseError, StatusCodes.Status500InternalServerError },
        };

        /// <summary>
        /// Maps a DomainException to HTTP status code
        /// Priority: Exception type > Error code > Default
        /// </summary>
        public static int MapToStatusCode(IBusinessException exception)
        {
            return MapByErrorCode(exception.Code);
        }

        /// <summary>
        /// Maps exception type to HTTP status code
        /// </summary>
        public static int MapByErrorCode(string errorCode)
        {
            if (_errorCodeToHttpStatus.TryGetValue(errorCode, out var statusCode))
            {
                return statusCode;
            }

            // Default to 500 for unknown error codes
            return StatusCodes.Status500InternalServerError;
        }

        /// <summary>
        /// Converts error code to HTTP status code (fallback method)
        /// </summary>
        public static int ToHttpStatusCode(string errorCode)
        {
            if (_errorCodeToHttpStatus.TryGetValue(errorCode, out var statusCode))
            {
                return statusCode;
            }

            // Default to 500 Internal Server Error for unknown error codes
            return StatusCodes.Status500InternalServerError;
        }

        /// <summary>
        /// Gets a user-friendly description of the HTTP status code
        /// </summary>
        public static string GetStatusDescription(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Forbidden",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
                StatusCodes.Status500InternalServerError => "Internal Server Error",
                StatusCodes.Status503ServiceUnavailable => "Service Unavailable",
                _ => "Unknown Error"
            };
        }

        /// <summary>
        /// Determines if the status code represents a client error
        /// </summary>
        public static bool IsClientError(int statusCode)
        {
            return statusCode >= 400 && statusCode < 500;
        }

        /// <summary>
        /// Determines if the status code represents a server error
        /// </summary>
        public static bool IsServerError(int statusCode)
        {
            return statusCode >= 500;
        }
    }
}
