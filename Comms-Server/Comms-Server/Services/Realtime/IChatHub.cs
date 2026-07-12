using Comms_Server.DTOs.Message;

namespace Comms_Server.Services
{
	public interface IChatHub
	{
		Task RegisterConnectionAsync(Guid userId, string connectionId, IEnumerable<Guid> conversationIds);
		Task UnregisterConnectionAsync(Guid userId, string connectionId);
		Task AddUsersToConversationAsync(IEnumerable<Guid> userIds, Guid conversationId);
		Task SendMessageAsync(Guid conversationId, MessageDto message);
		Task NotifyTypingAsync(Guid conversationId, string excludingConnectionId, Guid userId, string username);
		Task NotifyStoppedTypingAsync(Guid conversationId, string excludingConnectionId, Guid userId);
	}
}
