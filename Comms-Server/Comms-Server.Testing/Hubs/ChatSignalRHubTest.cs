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
	public class ChatSignalRHubTest
	{
		private const string ConnectionId = "conn-1";

		private Mock<IMessageService> _messageService = null!;
		private Mock<IConversationService> _conversationService = null!;
		private Mock<IChatHub> _chatHub = null!;
		private Mock<HubCallerContext> _context = null!;
		private ChatSignalRHub _hub = null!;

		[SetUp]
		public void Setup()
		{
			_messageService = new Mock<IMessageService>();
			_conversationService = new Mock<IConversationService>();
			_chatHub = new Mock<IChatHub>();

			_context = new Mock<HubCallerContext>();
			_context.Setup(c => c.ConnectionId).Returns(ConnectionId);

			_hub = new ChatSignalRHub(_messageService.Object, _conversationService.Object, _chatHub.Object)
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

		// ── OnConnectedAsync ──────────────────────────────────────────────────────

		[Test]
		public async Task OnConnectedAsync_RegistersConnectionWithTheUsersConversationIds()
		{
			// Arrange
			var userId = Guid.NewGuid();
			SetupCaller(userId);
			var conversationIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
			_conversationService.Setup(s => s.GetUserConversationIdsAsync(userId)).ReturnsAsync(conversationIds);

			// Act
			await _hub.OnConnectedAsync();

			// Assert
			_chatHub.Verify(h => h.RegisterConnectionAsync(userId, ConnectionId,
				It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(conversationIds))), Times.Once);
		}

		[Test]
		public async Task OnConnectedAsync_WhenNameIdentifierClaimIsMissing_RegistersWithAnEmptyGuid()
		{
			// Arrange
			_context.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));
			_conversationService.Setup(s => s.GetUserConversationIdsAsync(Guid.Empty)).ReturnsAsync([]);

			// Act
			await _hub.OnConnectedAsync();

			// Assert
			_chatHub.Verify(h => h.RegisterConnectionAsync(Guid.Empty, ConnectionId, It.IsAny<IEnumerable<Guid>>()), Times.Once);
		}

		// ── OnDisconnectedAsync ───────────────────────────────────────────────────

		[Test]
		public async Task OnDisconnectedAsync_UnregistersTheConnection()
		{
			// Arrange
			var userId = Guid.NewGuid();
			SetupCaller(userId);

			// Act
			await _hub.OnDisconnectedAsync(null);

			// Assert
			_chatHub.Verify(h => h.UnregisterConnectionAsync(userId, ConnectionId), Times.Once);
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
			var message = new MessageDto { Id = Guid.NewGuid(), ConversationId = conversationId, SenderId = userId, Content = "Hello" };
			_messageService.Setup(s => s.CreateMessageAsync(conversationId, userId, "Hello")).ReturnsAsync(message);

			// Act
			await _hub.SendMessage(conversationId, "Hello");

			// Assert
			_chatHub.Verify(h => h.SendMessageAsync(conversationId, message), Times.Once);
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
			_chatHub.Verify(h => h.SendMessageAsync(It.IsAny<Guid>(), It.IsAny<MessageDto>()), Times.Never);
		}

		// ── StartTyping ───────────────────────────────────────────────────────────

		[Test]
		public async Task StartTyping_NotifiesTypingWithTheCallersDetailsExcludingItsOwnConnection()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			SetupCaller(userId, "alice");

			// Act
			await _hub.StartTyping(conversationId);

			// Assert
			_chatHub.Verify(h => h.NotifyTypingAsync(conversationId, ConnectionId, userId, "alice"), Times.Once);
		}

		[Test]
		public async Task StartTyping_WhenNameClaimIsMissing_NotifiesWithUnknownAsTheUsername()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			SetupCaller(userId, username: null);

			// Act
			await _hub.StartTyping(conversationId);

			// Assert
			_chatHub.Verify(h => h.NotifyTypingAsync(conversationId, ConnectionId, userId, "Unknown"), Times.Once);
		}

		// ── StopTyping ────────────────────────────────────────────────────────────

		[Test]
		public async Task StopTyping_NotifiesStoppedTypingExcludingItsOwnConnection()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			SetupCaller(userId);

			// Act
			await _hub.StopTyping(conversationId);

			// Assert
			_chatHub.Verify(h => h.NotifyStoppedTypingAsync(conversationId, ConnectionId, userId), Times.Once);
		}
	}
}
