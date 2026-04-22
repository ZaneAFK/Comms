using Comms_Server.DTOs.Message;

namespace Comms_Server.DTOs.Conversation
{
	public class ConversationDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public IEnumerable<ConversationMemberDto> Members { get; set; } = [];
		public MessageDto? LastMessage { get; set; }
	}
}
