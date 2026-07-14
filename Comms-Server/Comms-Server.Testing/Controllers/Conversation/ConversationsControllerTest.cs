using System.Security.Claims;
using Comms_Server.Controllers;
using Comms_Server.DTOs.Conversation;
using Comms_Server.DTOs.Message;
using Comms_Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Controllers
{
	[TestFixture]
	public class ConversationsControllerTest
	{
		private Mock<IConversationService> _conversationServiceMock = null!;
		private Mock<IMessageService> _messageServiceMock = null!;
		private ConversationsController _controller = null!;

		[SetUp]
		public void Setup()
		{
			_conversationServiceMock = new Mock<IConversationService>();
			_messageServiceMock = new Mock<IMessageService>();
			_controller = new ConversationsController(_conversationServiceMock.Object, _messageServiceMock.Object);
		}

		private void AuthenticateAs(Guid userId)
		{
			SetUser(new ClaimsPrincipal(new ClaimsIdentity(
				new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
				"TestAuth")));
		}

		private void AuthenticateWithoutUserIdClaim()
		{
			SetUser(new ClaimsPrincipal(new ClaimsIdentity("TestAuth")));
		}

		private void AuthenticateWithInvalidUserIdClaim()
		{
			SetUser(new ClaimsPrincipal(new ClaimsIdentity(
				new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") },
				"TestAuth")));
		}

		private void SetUser(ClaimsPrincipal principal)
		{
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = principal }
			};
		}

		// ── GetConversations ─────────────────────────────────────────────────────

		[Test]
		public async Task GetConversations_WithAuthenticatedUser_ReturnsOkWithConversations()
		{
			// Arrange
			var userId = Guid.NewGuid();
			AuthenticateAs(userId);

			var conversations = new List<ConversationDto> { new() { Id = Guid.NewGuid(), Name = "Convo" } };
			_conversationServiceMock
				.Setup(x => x.GetUserConversationsAsync(userId))
				.ReturnsAsync(conversations);

			// Act
			var result = await _controller.GetConversations();

			// Assert
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult, "An authenticated request should result in an Ok response.");
			Assert.AreSame(conversations, okResult!.Value);
		}

		[Test]
		public async Task GetConversations_WithoutUserIdClaim_ReturnsUnauthorized()
		{
			// Arrange
			AuthenticateWithoutUserIdClaim();

			// Act
			var result = await _controller.GetConversations();

			// Assert
			Assert.IsInstanceOf<UnauthorizedResult>(result, "A missing user ID claim should result in an Unauthorized response.");
			_conversationServiceMock.Verify(x => x.GetUserConversationsAsync(It.IsAny<Guid>()), Times.Never);
		}

		[Test]
		public async Task GetConversations_WithInvalidUserIdClaim_ReturnsUnauthorized()
		{
			// Arrange
			AuthenticateWithInvalidUserIdClaim();

			// Act
			var result = await _controller.GetConversations();

			// Assert
			Assert.IsInstanceOf<UnauthorizedResult>(result, "A non-GUID user ID claim should result in an Unauthorized response.");
			_conversationServiceMock.Verify(x => x.GetUserConversationsAsync(It.IsAny<Guid>()), Times.Never);
		}

		// ── CreateConversation ───────────────────────────────────────────────────

		[Test]
		public async Task CreateConversation_WithValidRequest_ReturnsOkWithConversation()
		{
			// Arrange
			var userId = Guid.NewGuid();
			AuthenticateAs(userId);

			var memberIds = new List<Guid> { Guid.NewGuid() };
			var request = new CreateConversationRequest { Name = "New Convo", MemberIds = memberIds };
			var createdConversation = new ConversationDto { Id = Guid.NewGuid(), Name = "New Convo" };

			_conversationServiceMock
				.Setup(x => x.CreateConversationAsync("New Convo", memberIds, userId))
				.ReturnsAsync(createdConversation);

			// Act
			var result = await _controller.CreateConversation(request);

			// Assert
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult, "A successful creation should result in an Ok response.");
			Assert.AreSame(createdConversation, okResult!.Value);
		}

		[Test]
		public async Task CreateConversation_WhenServiceReturnsNull_ReturnsBadRequest()
		{
			// Arrange
			var userId = Guid.NewGuid();
			AuthenticateAs(userId);

			_conversationServiceMock
				.Setup(x => x.CreateConversationAsync(It.IsAny<string>(), It.IsAny<List<Guid>>(), It.IsAny<Guid>()))
				.ReturnsAsync((ConversationDto?)null);

			var request = new CreateConversationRequest { Name = "New Convo", MemberIds = [] };

			// Act
			var result = await _controller.CreateConversation(request);

			// Assert
			Assert.IsInstanceOf<BadRequestObjectResult>(result, "A failed creation should result in a BadRequest response.");
		}

		[Test]
		public async Task CreateConversation_WithoutUserIdClaim_ReturnsUnauthorized()
		{
			// Arrange
			AuthenticateWithoutUserIdClaim();
			var request = new CreateConversationRequest { Name = "New Convo", MemberIds = [] };

			// Act
			var result = await _controller.CreateConversation(request);

			// Assert
			Assert.IsInstanceOf<UnauthorizedResult>(result, "A missing user ID claim should result in an Unauthorized response.");
			_conversationServiceMock.Verify(
				x => x.CreateConversationAsync(It.IsAny<string>(), It.IsAny<List<Guid>>(), It.IsAny<Guid>()),
				Times.Never);
		}

		[Test]
		public async Task CreateConversation_CallsServiceWithRequestValuesAndAuthenticatedUser()
		{
			// Arrange
			var userId = Guid.NewGuid();
			AuthenticateAs(userId);

			var memberIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
			_conversationServiceMock
				.Setup(x => x.CreateConversationAsync(It.IsAny<string>(), It.IsAny<List<Guid>>(), It.IsAny<Guid>()))
				.ReturnsAsync(new ConversationDto { Id = Guid.NewGuid(), Name = "Group Chat" });

			var request = new CreateConversationRequest { Name = "Group Chat", MemberIds = memberIds };

			// Act
			await _controller.CreateConversation(request);

			// Assert
			_conversationServiceMock.Verify(x => x.CreateConversationAsync("Group Chat", memberIds, userId), Times.Once);
		}

		// ── GetMessages ──────────────────────────────────────────────────────────

		[Test]
		public async Task GetMessages_WhenUserIsMember_ReturnsOkWithMessages()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			AuthenticateAs(userId);

			_conversationServiceMock
				.Setup(x => x.IsUserMemberAsync(conversationId, userId))
				.ReturnsAsync(true);

			var messages = new List<MessageDto> { new() { Id = Guid.NewGuid(), ConversationId = conversationId } };
			_messageServiceMock
				.Setup(x => x.GetMessagesAsync(conversationId, 0, 50))
				.ReturnsAsync(messages);

			// Act
			var result = await _controller.GetMessages(conversationId);

			// Assert
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult, "A member requesting messages should receive an Ok response.");
			Assert.AreSame(messages, okResult!.Value);
		}

		[Test]
		public async Task GetMessages_WithCustomSkipAndTake_PassesValuesToService()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			AuthenticateAs(userId);

			_conversationServiceMock
				.Setup(x => x.IsUserMemberAsync(conversationId, userId))
				.ReturnsAsync(true);
			_messageServiceMock
				.Setup(x => x.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new List<MessageDto>());

			// Act
			await _controller.GetMessages(conversationId, skip: 10, take: 20);

			// Assert
			_messageServiceMock.Verify(x => x.GetMessagesAsync(conversationId, 10, 20), Times.Once);
		}

		[Test]
		public async Task GetMessages_WhenUserIsNotMember_ReturnsForbid()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			AuthenticateAs(userId);

			_conversationServiceMock
				.Setup(x => x.IsUserMemberAsync(conversationId, userId))
				.ReturnsAsync(false);

			// Act
			var result = await _controller.GetMessages(conversationId);

			// Assert
			Assert.IsInstanceOf<ForbidResult>(result, "A non-member requesting messages should receive a Forbid response.");
			_messageServiceMock.Verify(x => x.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
		}

		[Test]
		public async Task GetMessages_WithoutUserIdClaim_ReturnsUnauthorized()
		{
			// Arrange
			AuthenticateWithoutUserIdClaim();

			// Act
			var result = await _controller.GetMessages(Guid.NewGuid());

			// Assert
			Assert.IsInstanceOf<UnauthorizedResult>(result, "A missing user ID claim should result in an Unauthorized response.");
			_conversationServiceMock.Verify(x => x.IsUserMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
		}
	}
}
