using System.Security.Claims;
using Comms_Server.DTOs.Message;
using Comms_Server.Hubs;
using Comms_Server.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Hubs
{
	[TestFixture]
	public class ChatHubTest
	{
		private const string ConnectionId = "conn-1";

		private Mock<IMessageService> _messageService = null!;
		private Mock<IConversationService> _conversationService = null!;
		private Mock<IChatBroadcaster> _chatBroadcaster = null!;
		private Mock<HubCallerContext> _context = null!;
		private ChatHub _hub = null!;

		[SetUp]
		public void Setup()
		{
			_messageService = new Mock<IMessageService>();
			_conversationService = new Mock<IConversationService>();
			_chatBroadcaster = new Mock<IChatBroadcaster>();

			_context = new Mock<HubCallerContext>();
			_context.Setup(c => c.ConnectionId).Returns(ConnectionId);

			_hub = new ChatHub(_messageService.Object, _conversationService.Object, _chatBroadcaster.Object)
			{
				Context = _context.Object
			};
		}

		private void SetupCaller(Guid userId, string? username = "TestUser")
		{
			var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
			if (username != null)
			{
				claims.Add(new Claim(ClaimTypes.Name, username));
			}
			_context.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims)));
		}

		// ── SendMessage ───────────────────────────────────────────────────────────

		[Test]
		public async Task SendMessage_WhenCallerIsAMember_CreatesAndBroadcastsTheMessage()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			SetupCaller(userId);
			_conversationService.Setup(s => s.IsUserMemberAsync(conversationId, userId)).ReturnsAsync(true);
			var memberIds = new[] { userId, Guid.NewGuid() };
			_conversationService.Setup(s => s.GetConversationMemberIdsAsync(conversationId)).ReturnsAsync(memberIds);
			var message = new MessageDto { Id = Guid.NewGuid(), ConversationId = conversationId, SenderId = userId, Content = "Hello" };
			_messageService.Setup(s => s.CreateMessageAsync(conversationId, userId, "Hello")).ReturnsAsync(message);

			// Act
			await _hub.SendMessage(conversationId, "Hello");

			// Assert
			_chatBroadcaster.Verify(h => h.SendMessageAsync(memberIds, message), Times.Once);
		}

		[Test]
		public async Task SendMessage_WhenCallerIsNotAMember_DoesNotCreateOrBroadcastAMessage()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			SetupCaller(userId);
			_conversationService.Setup(s => s.IsUserMemberAsync(conversationId, userId)).ReturnsAsync(false);

			// Act
			await _hub.SendMessage(conversationId, "Hello");

			// Assert
			_messageService.Verify(s => s.CreateMessageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
			_chatBroadcaster.Verify(h => h.SendMessageAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<MessageDto>()), Times.Never);
		}

		// ── StartTyping ───────────────────────────────────────────────────────────

		[Test]
		public async Task StartTyping_NotifiesTypingWithTheCallersDetails()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			SetupCaller(userId, "alice");
			var memberIds = new[] { userId, Guid.NewGuid() };
			_conversationService.Setup(s => s.GetConversationMemberIdsAsync(conversationId)).ReturnsAsync(memberIds);

			// Act
			await _hub.StartTyping(conversationId);

			// Assert
			_chatBroadcaster.Verify(h => h.NotifyTypingAsync(conversationId, memberIds, userId, "alice"), Times.Once);
		}

		[Test]
		public async Task StartTyping_WhenNameClaimIsMissing_NotifiesWithUnknownAsTheUsername()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			SetupCaller(userId, username: null);
			var memberIds = new[] { userId };
			_conversationService.Setup(s => s.GetConversationMemberIdsAsync(conversationId)).ReturnsAsync(memberIds);

			// Act
			await _hub.StartTyping(conversationId);

			// Assert
			_chatBroadcaster.Verify(h => h.NotifyTypingAsync(conversationId, memberIds, userId, "Unknown"), Times.Once);
		}

		// ── StopTyping ────────────────────────────────────────────────────────────

		[Test]
		public async Task StopTyping_NotifiesStoppedTyping()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			SetupCaller(userId);
			var memberIds = new[] { userId, Guid.NewGuid() };
			_conversationService.Setup(s => s.GetConversationMemberIdsAsync(conversationId)).ReturnsAsync(memberIds);

			// Act
			await _hub.StopTyping(conversationId);

			// Assert
			_chatBroadcaster.Verify(h => h.NotifyStoppedTypingAsync(conversationId, memberIds, userId), Times.Once);
		}
	}
}
