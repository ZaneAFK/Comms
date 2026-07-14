namespace Comms_Server.Database.Models
{
	public class Conversation
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Name { get; set; } = string.Empty;
		public Guid CreatorId { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public ICollection<ConversationMember> Members { get; set; } = [];
		public ICollection<Message> Messages { get; set; } = [];
	}
}
