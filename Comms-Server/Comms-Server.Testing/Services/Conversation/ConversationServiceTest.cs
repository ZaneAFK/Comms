using Comms_Server.Database.Models;
using Comms_Server.Services;
using Comms_Server.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Comms_Server.Testing.Services
{
	[TestFixture]
	public class ConversationServiceTest : TransactionalTest
	{
		private ConversationService _conversationService = null!;
		private IMessageService _messageService = null!;
		private IUserService _userService = null!;

		[SetUp]
		public override async Task Setup()
		{
			await base.Setup();

			_conversationService = (ConversationService)_provider.GetRequiredService<IConversationService>();
			_messageService = _provider.GetRequiredService<IMessageService>();
			_userService = _provider.GetRequiredService<IUserService>();
		}

		private async Task<Guid> CreateTestUserAsync(string username, string email)
		{
			var result = await _userService.RegisterUserAsync(username, email, "supersecure123!");
			Assert.IsTrue(result.Succeeded, $"Precondition: user '{username}' creation should succeed.");
			return result.Value!.Id;
		}

		// ── GetUserConversationsAsync ──────────────────────────────────────────────

		[Test]
		public async Task GetUserConversationsAsync_WithNoConversations_ReturnsEmpty()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");

			// Act
			var result = await _conversationService.GetUserConversationsAsync(userId);

			// Assert
			Assert.IsEmpty(result, "A user with no conversations should receive an empty list.");
		}

		[Test]
		public async Task GetUserConversationsAsync_WithConversations_ReturnsAllConversations()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			await _conversationService.CreateConversationAsync("Convo 1", [], userId);
			await _conversationService.CreateConversationAsync("Convo 2", [], userId);

			// Act
			var result = await _conversationService.GetUserConversationsAsync(userId);

			// Assert
			Assert.AreEqual(2, result.Count(), "User should see all conversations they are a member of.");
		}

		[Test]
		public async Task GetUserConversationsAsync_OnlyReturnsConversationsForTheRequestingUser()
		{
			// Arrange
			var userId1 = await CreateTestUserAsync("User1", "user1@hotmail.com");
			var userId2 = await CreateTestUserAsync("User2", "user2@hotmail.com");

			await _conversationService.CreateConversationAsync("User1 Convo", [], userId1);
			await _conversationService.CreateConversationAsync("User2 Convo", [], userId2);

			// Act
			var result = await _conversationService.GetUserConversationsAsync(userId1);

			// Assert
			Assert.AreEqual(1, result.Count(), "User should only see conversations they are a member of.");
			Assert.AreEqual("User1 Convo", result.First().Name);
		}

		[Test]
		public async Task GetUserConversationsAsync_IncludesConversationMembers()
		{
			// Arrange
			var userId1 = await CreateTestUserAsync("User1", "user1@hotmail.com");
			var userId2 = await CreateTestUserAsync("User2", "user2@hotmail.com");

			await _conversationService.CreateConversationAsync("Group Chat", [userId2], userId1);

			// Act
			var result = await _conversationService.GetUserConversationsAsync(userId1);
			var convo = result.First();

			// Assert
			Assert.AreEqual(2, convo.Members.Count(), "Conversation should include all members.");
			Assert.IsTrue(convo.Members.Any(m => m.UserId == userId1), "Creator should be listed as a member.");
			Assert.IsTrue(convo.Members.Any(m => m.UserId == userId2), "Added member should be listed.");
		}

		[Test]
		public async Task GetUserConversationsAsync_WithNoMessages_LastMessageIsNull()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			await _conversationService.CreateConversationAsync("Empty Convo", [], userId);

			// Act
			var result = await _conversationService.GetUserConversationsAsync(userId);

			// Assert
			Assert.IsNull(result.First().LastMessage, "LastMessage should be null when no messages have been sent.");
		}

		[Test]
		public async Task GetUserConversationsAsync_WithMessages_ReturnsLastMessage()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await _conversationService.CreateConversationAsync("Chat", [], userId);

			await _messageService.CreateMessageAsync(convo!.Id, userId, "First message");
			await _messageService.CreateMessageAsync(convo.Id, userId, "Last message");

			// Act
			var result = await _conversationService.GetUserConversationsAsync(userId);

			// Assert
			Assert.IsNotNull(result.First().LastMessage, "LastMessage should not be null when messages exist.");
			Assert.AreEqual("Last message", result.First().LastMessage!.Content, "LastMessage should reflect the most recently sent message.");
		}

		// ── CreateConversationAsync ───────────────────────────────────────────────

		[Test]
		public async Task CreateConversationAsync_WithValidData_ReturnsConversationDto()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");

			// Act
			var result = await _conversationService.CreateConversationAsync("New Conversation", [], userId);

			// Assert
			Assert.IsNotNull(result, "CreateConversationAsync should return a ConversationDto on success.");
			Assert.AreEqual("New Conversation", result!.Name, "Returned DTO should have the correct name.");
			Assert.AreNotEqual(Guid.Empty, result.Id, "Returned DTO should have a valid ID.");
		}

		[Test]
		public async Task CreateConversationAsync_AlwaysIncludesCreatorAsMember()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");

			// Act
			var result = await _conversationService.CreateConversationAsync("New Conversation", [], userId);

			// Assert
			Assert.IsNotNull(result);
			Assert.IsTrue(result!.Members.Any(m => m.UserId == userId), "Creator should always be added as a member.");
		}

		[Test]
		public async Task CreateConversationAsync_WithAdditionalMembers_IncludesAllMembers()
		{
			// Arrange
			var creatorId = await CreateTestUserAsync("Creator", "creator@hotmail.com");
			var member1Id = await CreateTestUserAsync("Member1", "member1@hotmail.com");
			var member2Id = await CreateTestUserAsync("Member2", "member2@hotmail.com");

			// Act
			var result = await _conversationService.CreateConversationAsync("Group Chat", [member1Id, member2Id], creatorId);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(3, result!.Members.Count(), "All provided members plus the creator should be included.");
			Assert.IsTrue(result.Members.Any(m => m.UserId == creatorId));
			Assert.IsTrue(result.Members.Any(m => m.UserId == member1Id));
			Assert.IsTrue(result.Members.Any(m => m.UserId == member2Id));
		}

		[Test]
		public async Task CreateConversationAsync_DeduplicatesWhenCreatorIsAlsoInMemberIds()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");

			// Act
			var result = await _conversationService.CreateConversationAsync("Convo", [userId], userId);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result!.Members.Count(), "Creator should not be duplicated when included in the member ID list.");
		}

		[Test]
		public async Task CreateConversationAsync_PersistsConversationToDatabase()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");

			// Act
			var result = await _conversationService.CreateConversationAsync("Persisted Convo", [], userId);

			// Assert
			Assert.IsNotNull(result);
			var exists = await Factory.ExistsAsync<Conversation>(c => c.Id == result!.Id);
			Assert.IsTrue(exists, "Conversation should be persisted to the database.");
		}

		[Test]
		public async Task CreateConversationAsync_PersistsMembersToDatabase()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");

			// Act
			var result = await _conversationService.CreateConversationAsync("Convo", [], userId);

			// Assert
			Assert.IsNotNull(result);
			var memberExists = await Factory.ExistsAsync<ConversationMember>(
				cm => cm.ConversationId == result!.Id && cm.UserId == userId);
			Assert.IsTrue(memberExists, "ConversationMember record should be persisted to the database.");
		}

		// ── IsUserMemberAsync ─────────────────────────────────────────────────────

		[Test]
		public async Task IsUserMemberAsync_WhenUserIsMember_ReturnsTrue()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await _conversationService.CreateConversationAsync("Convo", [], userId);

			// Act
			var isMember = await _conversationService.IsUserMemberAsync(convo!.Id, userId);

			// Assert
			Assert.IsTrue(isMember, "IsUserMemberAsync should return true for a user who is a member.");
		}

		[Test]
		public async Task IsUserMemberAsync_WhenUserIsNotMember_ReturnsFalse()
		{
			// Arrange
			var creatorId = await CreateTestUserAsync("Creator", "creator@hotmail.com");
			var outsiderId = await CreateTestUserAsync("Outsider", "outsider@hotmail.com");
			var convo = await _conversationService.CreateConversationAsync("Convo", [], creatorId);

			// Act
			var isMember = await _conversationService.IsUserMemberAsync(convo!.Id, outsiderId);

			// Assert
			Assert.IsFalse(isMember, "IsUserMemberAsync should return false for a user who is not a member.");
		}

		[Test]
		public async Task IsUserMemberAsync_WithNonExistentConversation_ReturnsFalse()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");

			// Act
			var isMember = await _conversationService.IsUserMemberAsync(Guid.NewGuid(), userId);

			// Assert
			Assert.IsFalse(isMember, "IsUserMemberAsync should return false for a non-existent conversation.");
		}

		[Test]
		public async Task IsUserMemberAsync_AddedMemberIsRecognised()
		{
			// Arrange
			var creatorId = await CreateTestUserAsync("Creator", "creator@hotmail.com");
			var memberId = await CreateTestUserAsync("Member", "member@hotmail.com");
			var convo = await _conversationService.CreateConversationAsync("Convo", [memberId], creatorId);

			// Act
			var isMember = await _conversationService.IsUserMemberAsync(convo!.Id, memberId);

			// Assert
			Assert.IsTrue(isMember, "An explicitly added member should be recognised as a member.");
		}
	}
}
