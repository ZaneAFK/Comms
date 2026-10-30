using Comms_Server.Database;
using Comms_Server.Database.Models;
using Comms_Server.DTOs.Message;
using Comms_Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Services
{
	public class MessageService : IMessageService
	{
		private readonly CommsDbContext _db;
		private readonly ILogger<MessageService> _logger;

		public MessageService(CommsDbContext db, ILogger<MessageService> logger)
		{
			_db = db;
			_logger = logger;
		}

		public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, int skip, int take)
		{
			_logger.LogDebug("Fetching messages for conversation {ConversationId} (skip={Skip}, take={Take})", conversationId, skip, take);

			return await _db.Messages
				.Where(m => m.ConversationId == conversationId)
				.OrderBy(m => m.SentAt)
				.Skip(skip)
				.Take(take)
				.Select(Projections.ToMessageDto)
				.ToListAsync();
		}

		public async Task<MessageDto> CreateMessageAsync(Guid conversationId, Guid senderId, string content)
		{
			_logger.LogDebug("Creating message in conversation {ConversationId} from user {SenderId}", conversationId, senderId);

			var message = new Message
			{
				ConversationId = conversationId,
				SenderId = senderId,
				Content = content
			};

			_db.Messages.Add(message);
			await _db.SaveChangesAsync();

			_logger.LogDebug("Message {MessageId} created in conversation {ConversationId}", message.Id, conversationId);
			return await _db.Messages
				.Where(m => m.Id == message.Id)
				.Select(Projections.ToMessageDto)
				.SingleAsync();
		}
	}
}
