namespace Comms_Server.DTOs.Message
{
	public class MessageDto
	{
		public Guid Id { get; set; }
		public Guid ConversationId { get; set; }
		public Guid SenderId { get; set; }
		public string SenderUsername { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public DateTime SentAt { get; set; }
	}
}
