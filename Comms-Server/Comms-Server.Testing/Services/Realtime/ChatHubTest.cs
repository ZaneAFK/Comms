using Comms_Server.DTOs.Message;
using Comms_Server.Hubs;
using Comms_Server.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Services
{
	[TestFixture]
	public class ChatHubTest
	{
		private Mock<IUserConnectionTracker> _userConnectionTracker = null!;
		private Mock<IHubClients> _clients = null!;
		private Mock<IGroupManager> _groups = null!;
		private Mock<IClientProxy> _clientProxy = null!;
		private ChatHub _chatHub = null!;

		[SetUp]
		public void Setup()
		{
			_userConnectionTracker = new Mock<IUserConnectionTracker>();
			_clients = new Mock<IHubClients>();
			_groups = new Mock<IGroupManager>();
			_clientProxy = new Mock<IClientProxy>();

			_clients.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxy.Object);
			_clients.Setup(c => c.GroupExcept(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>())).Returns(_clientProxy.Object);

			var hubContext = new Mock<IHubContext<ChatSignalRHub>>();
			hubContext.Setup(h => h.Clients).Returns(_clients.Object);
			hubContext.Setup(h => h.Groups).Returns(_groups.Object);

			_chatHub = new ChatHub(hubContext.Object, _userConnectionTracker.Object);
		}

		// ── RegisterConnectionAsync ──────────────────────────────────────────────

		[Test]
		public async Task RegisterConnectionAsync_AddsConnectionToTracker()
		{
			// Arrange
			var userId = Guid.NewGuid();

			// Act
			await _chatHub.RegisterConnectionAsync(userId, "conn-1", []);

			// Assert
			_userConnectionTracker.Verify(t => t.AddConnection(userId, "conn-1"), Times.Once);
		}

		[Test]
		public async Task RegisterConnectionAsync_JoinsGroupForEachConversationId()
		{
			// Arrange
			var conversationId1 = Guid.NewGuid();
			var conversationId2 = Guid.NewGuid();

			// Act
			await _chatHub.RegisterConnectionAsync(Guid.NewGuid(), "conn-1", [conversationId1, conversationId2]);

			// Assert
			_groups.Verify(g => g.AddToGroupAsync("conn-1", conversationId1.ToString(), It.IsAny<CancellationToken>()), Times.Once);
			_groups.Verify(g => g.AddToGroupAsync("conn-1", conversationId2.ToString(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Test]
		public async Task RegisterConnectionAsync_WithNoConversationIds_DoesNotJoinAnyGroups()
		{
			// Act
			await _chatHub.RegisterConnectionAsync(Guid.NewGuid(), "conn-1", []);

			// Assert
			_groups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
		}

		// ── UnregisterConnectionAsync ─────────────────────────────────────────────

		[Test]
		public async Task UnregisterConnectionAsync_RemovesConnectionFromTracker()
		{
			// Arrange
			var userId = Guid.NewGuid();

			// Act
			await _chatHub.UnregisterConnectionAsync(userId, "conn-1");

			// Assert
			_userConnectionTracker.Verify(t => t.RemoveConnection(userId, "conn-1"), Times.Once);
		}

		// ── AddUsersToConversationAsync ───────────────────────────────────────────

		[Test]
		public async Task AddUsersToConversationAsync_AddsEachTrackedConnectionToGroup()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			_userConnectionTracker.Setup(t => t.GetConnections(userId)).Returns(["conn-1", "conn-2"]);

			// Act
			await _chatHub.AddUsersToConversationAsync([userId], conversationId);

			// Assert
			_groups.Verify(g => g.AddToGroupAsync("conn-1", conversationId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
			_groups.Verify(g => g.AddToGroupAsync("conn-2", conversationId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Test]
		public async Task AddUsersToConversationAsync_WithMultipleUsers_AddsConnectionsForEachUser()
		{
			// Arrange
			var userId1 = Guid.NewGuid();
			var userId2 = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			_userConnectionTracker.Setup(t => t.GetConnections(userId1)).Returns(["conn-1"]);
			_userConnectionTracker.Setup(t => t.GetConnections(userId2)).Returns(["conn-2"]);

			// Act
			await _chatHub.AddUsersToConversationAsync([userId1, userId2], conversationId);

			// Assert
			_groups.Verify(g => g.AddToGroupAsync("conn-1", conversationId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
			_groups.Verify(g => g.AddToGroupAsync("conn-2", conversationId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Test]
		public async Task AddUsersToConversationAsync_UserWithNoActiveConnections_AddsNothing()
		{
			// Arrange
			var userId = Guid.NewGuid();
			_userConnectionTracker.Setup(t => t.GetConnections(userId)).Returns([]);

			// Act
			await _chatHub.AddUsersToConversationAsync([userId], Guid.NewGuid());

			// Assert
			_groups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
		}

		// ── SendMessageAsync ──────────────────────────────────────────────────────

		[Test]
		public async Task SendMessageAsync_SendsReceiveMessageToTheConversationGroup()
		{
			// Arrange
			var conversationId = Guid.NewGuid();
			var message = new MessageDto { Id = Guid.NewGuid(), ConversationId = conversationId, Content = "Hi" };

			// Act
			await _chatHub.SendMessageAsync(conversationId, message);

			// Assert
			_clients.Verify(c => c.Group(conversationId.ToString()), Times.Once);
			_clientProxy.Verify(p => p.SendCoreAsync("ReceiveMessage",
				It.Is<object?[]>(args => args.Length == 1 && args[0] == message),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		// ── NotifyTypingAsync ─────────────────────────────────────────────────────

		[Test]
		public async Task NotifyTypingAsync_SendsUserTypingToTheGroupExcludingTheCaller()
		{
			// Arrange
			var conversationId = Guid.NewGuid();
			var userId = Guid.NewGuid();

			// Act
			await _chatHub.NotifyTypingAsync(conversationId, "caller-conn", userId, "alice");

			// Assert
			_clients.Verify(c => c.GroupExcept(conversationId.ToString(),
				It.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == "caller-conn")), Times.Once);
			_clientProxy.Verify(p => p.SendCoreAsync("UserTyping", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		// ── NotifyStoppedTypingAsync ──────────────────────────────────────────────

		[Test]
		public async Task NotifyStoppedTypingAsync_SendsUserStoppedTypingToTheGroupExcludingTheCaller()
		{
			// Arrange
			var conversationId = Guid.NewGuid();
			var userId = Guid.NewGuid();

			// Act
			await _chatHub.NotifyStoppedTypingAsync(conversationId, "caller-conn", userId);

			// Assert
			_clients.Verify(c => c.GroupExcept(conversationId.ToString(),
				It.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == "caller-conn")), Times.Once);
			_clientProxy.Verify(p => p.SendCoreAsync("UserStoppedTyping", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
