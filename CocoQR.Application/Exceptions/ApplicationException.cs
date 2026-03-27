using CocoQR.Domain.Exceptions;

namespace CocoQR.Application.Exceptions
{
    public class ApplicationException : Exception, IBusinessException
    {
        public string Code { get; }
        public object? Data { get; }

        public ApplicationException(
            string code,
            string message,
            object? data = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Code = code;
            Data = data;
        }
    }
}
