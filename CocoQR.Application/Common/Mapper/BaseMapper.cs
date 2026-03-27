namespace CocoQR.Application.Common.Mapper
{
    public class BaseMapper
    {
        public static string? GetUserName(
        Guid? userId,
        IReadOnlyDictionary<Guid, string>? dict)
        {
            if (userId == null || userId == Guid.Empty) { return null; }
            return dict.TryGetValue(userId.Value, out var name) ? name : null;
        }
    }
}
