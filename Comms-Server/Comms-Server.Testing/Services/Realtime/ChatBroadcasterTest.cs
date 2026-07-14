using Comms_Server.DTOs.Message;
using Comms_Server.Hubs;
using Comms_Server.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Services
{
	[TestFixture]
	public class ChatBroadcasterTest
	{
		private Mock<IHubClients> _clients = null!;
		private Mock<IClientProxy> _clientProxy = null!;
		private ChatBroadcaster _chatBroadcaster = null!;

		[SetUp]
		public void Setup()
		{
			_clients = new Mock<IHubClients>();
			_clientProxy = new Mock<IClientProxy>();

			_clients.Setup(c => c.Users(It.IsAny<IReadOnlyList<string>>())).Returns(_clientProxy.Object);

			var hubContext = new Mock<IHubContext<ChatHub>>();
			hubContext.Setup(h => h.Clients).Returns(_clients.Object);

			_chatBroadcaster = new ChatBroadcaster(hubContext.Object);
		}

		// ── SendMessageAsync ──────────────────────────────────────────────────────

		[Test]
		public async Task SendMessageAsync_SendsReceiveMessageToEveryMember()
		{
			// Arrange
			var conversationId = Guid.NewGuid();
			var member1 = Guid.NewGuid();
			var member2 = Guid.NewGuid();
			var message = new MessageDto { Id = Guid.NewGuid(), ConversationId = conversationId, Content = "Hi" };

			// Act
			await _chatBroadcaster.SendMessageAsync([member1, member2], message);

			// Assert
			_clients.Verify(c => c.Users(It.Is<IReadOnlyList<string>>(ids =>
				ids.Count == 2 && ids.Contains(member1.ToString()) && ids.Contains(member2.ToString()))), Times.Once);
			_clientProxy.Verify(p => p.SendCoreAsync("ReceiveMessage",
				It.Is<object?[]>(args => args.Length == 1 && args[0] == message),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		// ── NotifyTypingAsync ─────────────────────────────────────────────────────

		[Test]
		public async Task NotifyTypingAsync_SendsUserTypingToMembersExcludingTheCaller()
		{
			// Arrange
			var conversationId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var otherMember = Guid.NewGuid();

			// Act
			await _chatBroadcaster.NotifyTypingAsync(conversationId, [userId, otherMember], userId, "alice");

			// Assert
			_clients.Verify(c => c.Users(It.Is<IReadOnlyList<string>>(ids =>
				ids.Count == 1 && ids[0] == otherMember.ToString())), Times.Once);
			_clientProxy.Verify(p => p.SendCoreAsync("UserTyping", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		// ── NotifyStoppedTypingAsync ──────────────────────────────────────────────

		[Test]
		public async Task NotifyStoppedTypingAsync_SendsUserStoppedTypingToMembersExcludingTheCaller()
		{
			// Arrange
			var conversationId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var otherMember = Guid.NewGuid();

			// Act
			await _chatBroadcaster.NotifyStoppedTypingAsync(conversationId, [userId, otherMember], userId);

			// Assert
			_clients.Verify(c => c.Users(It.Is<IReadOnlyList<string>>(ids =>
				ids.Count == 1 && ids[0] == otherMember.ToString())), Times.Once);
			_clientProxy.Verify(p => p.SendCoreAsync("UserStoppedTyping", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
