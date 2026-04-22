using Comms_Server.DTOs.Message;

namespace Comms_Server.Services
{
	public interface IMessageService
	{
		Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, int skip, int take);
		Task<MessageDto> CreateMessageAsync(Guid conversationId, Guid senderId, string content);
	}
}
