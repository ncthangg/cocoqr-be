namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker
{
    public class NonRetryableJobException : Exception
    {
        public NonRetryableJobException(string message) : base(message)
        {
        }

        public NonRetryableJobException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
