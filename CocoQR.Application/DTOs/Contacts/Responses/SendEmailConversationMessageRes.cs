namespace CocoQR.Application.DTOs.Contacts.Responses
{
    public sealed class SendEmailConversationMessageRes
    {
        public Guid ConversationId { get; set; }
        public Guid MessageId { get; set; }
        public int SequenceNumber { get; set; }
    }
}
