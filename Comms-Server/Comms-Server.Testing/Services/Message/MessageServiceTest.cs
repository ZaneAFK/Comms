using Comms_Server.Database.Models;
using Comms_Server.DTOs.Conversation;
using Comms_Server.Services;
using Comms_Server.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Comms_Server.Testing.Services
{
	[TestFixture]
	public class MessageServiceTest : TransactionalTest
	{
		private MessageService _messageService = null!;
		private IConversationService _conversationService = null!;
		private IUserService _userService = null!;

		[SetUp]
		public override async Task Setup()
		{
			await base.Setup();

			_messageService = (MessageService)_provider.GetRequiredService<IMessageService>();
			_conversationService = _provider.GetRequiredService<IConversationService>();
			_userService = _provider.GetRequiredService<IUserService>();
		}

		private async Task<Guid> CreateTestUserAsync(string username, string email)
		{
			var result = await _userService.RegisterUserAsync(username, email, "supersecure123!");
			Assert.IsTrue(result.Succeeded, $"Precondition: user '{username}' creation should succeed.");
			return result.Value!.Id;
		}

		private async Task<ConversationDto> CreateTestConversationAsync(Guid creatorId, string name = "Test Conversation")
		{
			var result = await _conversationService.CreateConversationAsync(name, [], creatorId);
			Assert.IsNotNull(result, "Precondition: conversation creation should succeed.");
			return result!;
		}

		// ── CreateMessageAsync ────────────────────────────────────────────────────

		[Test]
		public async Task CreateMessageAsync_WithValidData_ReturnsPopulatedMessageDto()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			// Act
			var result = await _messageService.CreateMessageAsync(convo.Id, userId, "Hello, world!");

			// Assert
			Assert.IsNotNull(result);
			Assert.AreNotEqual(Guid.Empty, result.Id, "Returned message should have a valid ID.");
			Assert.AreEqual(convo.Id, result.ConversationId, "Returned message should reference the correct conversation.");
			Assert.AreEqual(userId, result.SenderId, "Returned message should reference the correct sender.");
			Assert.AreEqual("Hello, world!", result.Content, "Returned message should contain the correct content.");
		}

		[Test]
		public async Task CreateMessageAsync_ReturnsSenderUsername()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			// Act
			var result = await _messageService.CreateMessageAsync(convo.Id, userId, "Hi!");

			// Assert
			Assert.AreEqual("TestUser", result.SenderUsername, "Returned message should include the sender's username.");
		}

		[Test]
		public async Task CreateMessageAsync_PersistsMessageToDatabase()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			// Act
			var result = await _messageService.CreateMessageAsync(convo.Id, userId, "Persisted!");

			// Assert
			var exists = await Factory.ExistsAsync<Message>(m => m.Id == result.Id);
			Assert.IsTrue(exists, "Message should be persisted to the database.");
		}

		[Test]
		public async Task CreateMessageAsync_MultipleMessages_EachPersisted()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			// Act
			var msg1 = await _messageService.CreateMessageAsync(convo.Id, userId, "First");
			var msg2 = await _messageService.CreateMessageAsync(convo.Id, userId, "Second");

			// Assert
			Assert.AreNotEqual(msg1.Id, msg2.Id, "Each message should receive a unique ID.");
			Assert.IsTrue(await Factory.ExistsAsync<Message>(m => m.Id == msg1.Id));
			Assert.IsTrue(await Factory.ExistsAsync<Message>(m => m.Id == msg2.Id));
		}

		// ── GetMessagesAsync ──────────────────────────────────────────────────────

		[Test]
		public async Task GetMessagesAsync_WithNoMessages_ReturnsEmpty()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			// Act
			var result = await _messageService.GetMessagesAsync(convo.Id, 0, 50);

			// Assert
			Assert.IsEmpty(result, "GetMessagesAsync should return an empty list when no messages exist.");
		}

		[Test]
		public async Task GetMessagesAsync_ReturnsAllMessagesWithinTake()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			await _messageService.CreateMessageAsync(convo.Id, userId, "Message 1");
			await _messageService.CreateMessageAsync(convo.Id, userId, "Message 2");
			await _messageService.CreateMessageAsync(convo.Id, userId, "Message 3");

			// Act
			var result = await _messageService.GetMessagesAsync(convo.Id, 0, 50);

			// Assert
			Assert.AreEqual(3, result.Count(), "All messages in the conversation should be returned.");
		}

		[Test]
		public async Task GetMessagesAsync_ReturnsMessagesInChronologicalOrder()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			await _messageService.CreateMessageAsync(convo.Id, userId, "First");
			await _messageService.CreateMessageAsync(convo.Id, userId, "Second");
			await _messageService.CreateMessageAsync(convo.Id, userId, "Third");

			// Act
			var messages = (await _messageService.GetMessagesAsync(convo.Id, 0, 50)).ToList();

			// Assert
			for (var i = 1; i < messages.Count; i++)
			{
				Assert.IsTrue(messages[i].SentAt >= messages[i - 1].SentAt,
					"Messages should be returned in non-decreasing chronological order.");
			}
		}

		[Test]
		public async Task GetMessagesAsync_RespectsSkip()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			await _messageService.CreateMessageAsync(convo.Id, userId, "First");
			await _messageService.CreateMessageAsync(convo.Id, userId, "Second");
			await _messageService.CreateMessageAsync(convo.Id, userId, "Third");

			// Act
			var result = (await _messageService.GetMessagesAsync(convo.Id, 2, 50)).ToList();

			// Assert
			Assert.AreEqual(1, result.Count, "Skip should exclude the first two messages.");
		}

		[Test]
		public async Task GetMessagesAsync_RespectsTake()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			await _messageService.CreateMessageAsync(convo.Id, userId, "First");
			await _messageService.CreateMessageAsync(convo.Id, userId, "Second");
			await _messageService.CreateMessageAsync(convo.Id, userId, "Third");

			// Act
			var result = await _messageService.GetMessagesAsync(convo.Id, 0, 2);

			// Assert
			Assert.AreEqual(2, result.Count(), "Take should limit the number of messages returned.");
		}

		[Test]
		public async Task GetMessagesAsync_RespectsSkipAndTakeTogether()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			for (var i = 1; i <= 5; i++)
			{
				await _messageService.CreateMessageAsync(convo.Id, userId, $"Message {i}");
			}

			// Act — skip first 2, take next 2
			var result = await _messageService.GetMessagesAsync(convo.Id, 2, 2);

			// Assert
			Assert.AreEqual(2, result.Count(), "Skip and take together should return the correct page of messages.");
		}

		[Test]
		public async Task GetMessagesAsync_OnlyReturnsMessagesForSpecifiedConversation()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo1 = await CreateTestConversationAsync(userId, "Convo 1");
			var convo2 = await CreateTestConversationAsync(userId, "Convo 2");

			await _messageService.CreateMessageAsync(convo1.Id, userId, "Convo1 message");
			await _messageService.CreateMessageAsync(convo2.Id, userId, "Convo2 message");

			// Act
			var result = (await _messageService.GetMessagesAsync(convo1.Id, 0, 50)).ToList();

			// Assert
			Assert.AreEqual(1, result.Count, "GetMessagesAsync should only return messages for the specified conversation.");
			Assert.AreEqual("Convo1 message", result[0].Content);
		}

		[Test]
		public async Task GetMessagesAsync_PopulatesAllDtoFields()
		{
			// Arrange
			var userId = await CreateTestUserAsync("TestUser", "testuser@hotmail.com");
			var convo = await CreateTestConversationAsync(userId);

			await _messageService.CreateMessageAsync(convo.Id, userId, "Check fields");

			// Act
			var result = (await _messageService.GetMessagesAsync(convo.Id, 0, 50)).First();

			// Assert
			Assert.AreNotEqual(Guid.Empty, result.Id);
			Assert.AreEqual(convo.Id, result.ConversationId);
			Assert.AreEqual(userId, result.SenderId);
			Assert.AreEqual("TestUser", result.SenderUsername);
			Assert.AreEqual("Check fields", result.Content);
			Assert.AreNotEqual(default(DateTime), result.SentAt);
		}
	}
}
