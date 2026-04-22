using Comms_Server.DTOs.Conversation;

namespace Comms_Server.Services
{
	public interface IConversationService
	{
		Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(Guid userId);
		Task<ConversationDto?> CreateConversationAsync(string name, List<Guid> memberIds, Guid creatorId);
		Task<bool> IsUserMemberAsync(Guid conversationId, Guid userId);
	}
}
