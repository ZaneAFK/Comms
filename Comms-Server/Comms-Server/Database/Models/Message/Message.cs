namespace Comms_Server.Database.Models
{
	public class Message
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid ConversationId { get; set; }
		public Conversation Conversation { get; set; } = null!;
		public Guid SenderId { get; set; }
		public User Sender { get; set; } = null!;
		public string Content { get; set; } = string.Empty;
		public DateTime SentAt { get; set; } = DateTime.UtcNow;
	}
}
