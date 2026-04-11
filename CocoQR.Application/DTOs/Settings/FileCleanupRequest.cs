namespace CocoQR.Application.DTOs.Settings
{
    public sealed class FileCleanupRequest
    {
        public required string FilePath { get; init; }
        public bool DeleteCloud { get; init; }
        public bool DeleteLocal { get; init; }
        public int Attempt { get; init; }
    }
}
