using Comms_Server.DTOs.Message;

namespace Comms_Server.Services
{
	public interface IChatBroadcaster
	{
		Task SendMessageAsync(IEnumerable<Guid> memberIds, MessageDto message);
		Task NotifyTypingAsync(Guid conversationId, IEnumerable<Guid> memberIds, Guid userId, string username);
		Task NotifyStoppedTypingAsync(Guid conversationId, IEnumerable<Guid> memberIds, Guid userId);
	}
}
