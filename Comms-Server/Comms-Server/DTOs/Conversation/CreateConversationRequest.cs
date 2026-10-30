namespace Comms_Server.DTOs.Conversation
{
	public class CreateConversationRequest
	{
		public string Name { get; set; } = string.Empty;
		public List<Guid> MemberIds { get; set; } = [];
	}
}
