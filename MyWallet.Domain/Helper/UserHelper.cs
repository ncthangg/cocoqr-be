using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories;

namespace MyWallet.Domain.Helper
{
    public static class UserHelper
    {
        public static async Task<Dictionary<Guid, string>> GetUserNameDictAsync(BaseEntity item, IUserRepository userRepository)
        {
            var userIds = new Guid?[] { item.CreatedBy, item.UpdatedBy, item.DeletedBy }
            .Where(id => id.HasValue && id.Value != Guid.Empty).Select(id => id!.Value).Distinct().ToList();

            if (userIds.Count == 0)
                return new Dictionary<Guid, string>();

            var users = await userRepository.GetUsersByIdsAsync(userIds);

            return users.ToDictionary(u => u.Id, u => u.FullName);
        }
        public static async Task<Dictionary<Guid, string>> GetUserNameDictAsync<T>(List<T> items, IUserRepository userRepository) where T : BaseEntity
        {
            var userIds = items.SelectMany(p => new Guid?[] { p.CreatedBy, p.UpdatedBy, p.DeletedBy })
                .Where(id => id.HasValue && id.Value != Guid.Empty).Select(id => id!.Value).Distinct().ToList();

            if (userIds.Count == 0)
                return new Dictionary<Guid, string>();

            var users = await userRepository.GetUsersByIdsAsync(userIds);
            return users.ToDictionary(u => u.Id, u => u.FullName);
        }
    }
}
