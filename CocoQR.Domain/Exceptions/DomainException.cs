namespace CocoQR.Domain.Exceptions
{
    public interface IBusinessException
    {
        string Code { get; }
        string Message { get; }
        object? Details { get; }
    }
    public class DomainException : Exception, IBusinessException
    {
        public string Code { get; }
        public object? Details { get; }

        public DomainException(
            string code,
            string message,
            object? data = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Code = code;
            Details = data;
        }
    }
}
