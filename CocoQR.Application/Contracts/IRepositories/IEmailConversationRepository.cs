using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Application.DTOs.Contacts.Queries;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IEmailConversationRepository : IRepository<EmailConversation>
    {
        Task<(IEnumerable<ContactConversationQueryDto> Items, int TotalCount)> GetPagedForAdminAsync(
            int pageNumber,
            int pageSize,
            string? sortField,
            string? sortDirection,
            Guid? userId,
            string? searchValue,
            ContactMessageStatus? contactStatus,
            DateTime? fromDate,
            DateTime? toDate);
        Task<EmailConversation?> GetByContactMessageIdAsync(Guid contactMessageId);
        Task UpdateLastMessageAtAsync(Guid conversationId, DateTime lastMessageAt);
    }
}
