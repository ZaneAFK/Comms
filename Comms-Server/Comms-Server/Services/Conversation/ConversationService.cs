using Comms_Server.Database;
using Comms_Server.Database.Models;
using Comms_Server.DTOs.Conversation;
using Comms_Server.DTOs.Message;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Services
{
	public class ConversationService : Service<ConversationService>, IConversationService
	{
		public ConversationService(IFactory factory, ILogger<ConversationService> logger) : base(factory, logger)
		{
		}

		public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(Guid userId)
		{
			Logger.LogDebug("Retrieving conversations for user {UserId}", userId);

			var conversations = await Factory.Query<ConversationMember>()
				.Where(cm => cm.UserId == userId)
				.Include(cm => cm.Conversation)
					.ThenInclude(c => c.Members)
						.ThenInclude(m => m.User)
				.Select(cm => cm.Conversation)
				.ToListAsync();

			return await PopulateConversationDtos(conversations);
		}

		public async Task<ConversationDto?> CreateConversationAsync(string name, List<Guid> memberIds, Guid creatorId)
		{
			var allMemberIds = memberIds.Union([creatorId]).Distinct().ToList();
			Logger.LogInformation("Creating conversation '{Name}' with {MemberCount} member(s)", name, allMemberIds.Count);

			using var transaction = await Factory.BeginTransactionAsync();

			var conversation = CreateConversationAndLinkToMembers(name, allMemberIds);

			await Factory.SaveAsync();
			await transaction.CommitAsync();

			var created = await Factory.Query<Conversation>()
				.Include(c => c.Members)
					.ThenInclude(m => m.User)
				.FirstOrDefaultAsync(c => c.Id == conversation.Id);

			if (created == null)
			{
				Logger.LogWarning("Conversation '{Name}' was saved but could not be retrieved (ID: {ConversationId})", name, conversation.Id);
				return null;
			}

			Logger.LogInformation("Conversation '{Name}' created with ID {ConversationId}", created.Name, created.Id);
			return new ConversationDto
			{
				Id = created.Id,
				Name = created.Name,
				CreatedAt = created.CreatedAt,
				Members = created.Members.Select(m => new ConversationMemberDto
				{
					UserId = m.UserId,
					Username = m.User.UserName!
				})
			};
		}

		public async Task<bool> IsUserMemberAsync(Guid conversationId, Guid userId)
		{
			return await Factory.ExistsAsync<ConversationMember>(
				cm => cm.ConversationId == conversationId && cm.UserId == userId);
		}

		async Task<List<ConversationDto>> PopulateConversationDtos(List<Conversation> conversations)
		{
			var result = new List<ConversationDto>();

			foreach (var convo in conversations)
			{
				var lastMessage = await Factory.Query<Message>()
						.Where(m => m.ConversationId == convo.Id)
						.Include(m => m.Sender)
						.OrderByDescending(m => m.SentAt)
						.Select(m => new MessageDto
						{
							Id = m.Id,
							ConversationId = m.ConversationId,
							SenderId = m.SenderId,
							SenderUsername = m.Sender.UserName!,
							Content = m.Content,
							SentAt = m.SentAt
						})
						.FirstOrDefaultAsync();

				result.Add(new ConversationDto
				{
					Id = convo.Id,
					Name = convo.Name,
					CreatedAt = convo.CreatedAt,
					Members = convo.Members.Select(m => new ConversationMemberDto
					{
						UserId = m.UserId,
						Username = m.User.UserName!
					}),
					LastMessage = lastMessage
				});
			}

			return result;
		}

		Conversation CreateConversationAndLinkToMembers(string name, List<Guid> memberIds)
		{
			var conversation = Factory.New<Conversation>();
			conversation.Name = name;

			foreach (var id in memberIds)
			{
				var convoMember = Factory.New<ConversationMember>();
				convoMember.ConversationId = conversation.Id;
				convoMember.UserId = id;
			}

			return conversation;
		}
	}
}
