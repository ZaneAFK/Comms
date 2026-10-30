using Comms_Server.Database;
using Comms_Server.Database.Models;
using Comms_Server.DTOs.Conversation;
using Comms_Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Services
{
	public class ConversationService : IConversationService
	{
		private readonly CommsDbContext _db;
		private readonly ILogger<ConversationService> _logger;

		public ConversationService(CommsDbContext db, ILogger<ConversationService> logger)
		{
			_db = db;
			_logger = logger;
		}

		public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(Guid userId)
		{
			_logger.LogDebug("Retrieving conversations for user {UserId}", userId);

			return await _db.Conversations
				.Where(c => c.Members.Any(m => m.UserId == userId) && (c.CreatorId == userId || c.Messages.Any()))
				.Select(c => new ConversationDto
				{
					Id = c.Id,
					Name = c.Name,
					CreatedAt = c.CreatedAt,
					Members = c.Members.Select(m => new ConversationMemberDto
					{
						UserId = m.UserId,
						Username = m.User.UserName!
					}).ToList(),
					LastMessage = c.Messages.AsQueryable()
						.OrderByDescending(m => m.SentAt)
						.Select(Projections.ToMessageDto)
						.FirstOrDefault()
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<Guid>> GetConversationMemberIdsAsync(Guid conversationId)
		{
			return await _db.ConversationMembers
				.Where(cm => cm.ConversationId == conversationId)
				.Select(cm => cm.UserId)
				.ToListAsync();
		}

		public async Task<ConversationDto?> CreateConversationAsync(string name, List<Guid> memberIds, Guid creatorId)
		{
			var allMemberIds = memberIds.Union([creatorId]).Distinct().ToList();
			_logger.LogDebug("Creating conversation '{Name}' with {MemberCount} member(s)", name, allMemberIds.Count);

			var conversation = new Conversation
			{
				Name = name,
				CreatorId = creatorId,
				Members = allMemberIds.Select(id => new ConversationMember { UserId = id }).ToList()
			};

			_db.Conversations.Add(conversation);
			await _db.SaveChangesAsync();

			var created = await _db.Conversations
				.Where(c => c.Id == conversation.Id)
				.Select(c => new ConversationDto
				{
					Id = c.Id,
					Name = c.Name,
					CreatedAt = c.CreatedAt,
					Members = c.Members.Select(m => new ConversationMemberDto
					{
						UserId = m.UserId,
						Username = m.User.UserName!
					}).ToList()
				})
				.FirstOrDefaultAsync();

			if (created == null)
			{
				_logger.LogWarning("Conversation '{Name}' was saved but could not be retrieved (ID: {ConversationId})", name, conversation.Id);
				return null;
			}

			_logger.LogDebug("Conversation '{Name}' created with ID {ConversationId}", created.Name, created.Id);
			return created;
		}

		public async Task<bool> IsUserMemberAsync(Guid conversationId, Guid userId)
		{
			return await _db.ConversationMembers
				.AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
		}
	}
}
