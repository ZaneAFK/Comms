using Comms_Server.Database;
using Comms_Server.Database.Models;
using Comms_Server.DTOs.Message;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Services
{
	public class MessageService : Service, IMessageService
	{
		public MessageService(IFactory factory) : base(factory)
		{
		}

		public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, int skip, int take)
		{
			return await Factory.Query<Message>()
				.Where(m => m.ConversationId == conversationId)
				.Include(m => m.Sender)
				.OrderBy(m => m.SentAt)
				.Skip(skip)
				.Take(take)
				.Select(m => new MessageDto
				{
					Id = m.Id,
					ConversationId = m.ConversationId,
					SenderId = m.SenderId,
					SenderUsername = m.Sender.UserName!,
					Content = m.Content,
					SentAt = m.SentAt
				})
				.ToListAsync();
		}

		public async Task<MessageDto> CreateMessageAsync(Guid conversationId, Guid senderId, string content)
		{
			var message = Factory.New<Message>();

			message.ConversationId = conversationId;
			message.SenderId = senderId;
			message.Content = content;

			await Factory.SaveAsync();

			var sender = await Factory.GetAsync<User>(senderId);

			return new MessageDto
			{
				Id = message.Id,
				ConversationId = message.ConversationId,
				SenderId = message.SenderId,
				SenderUsername = sender?.UserName ?? "Unknown",
				Content = message.Content,
				SentAt = message.SentAt
			};
		}
	}
}
