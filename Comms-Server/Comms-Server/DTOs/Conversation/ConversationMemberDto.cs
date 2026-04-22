namespace Comms_Server.DTOs.Conversation
{
	public class ConversationMemberDto
	{
		public Guid UserId { get; set; }
		public string Username { get; set; } = string.Empty;
	}
}
