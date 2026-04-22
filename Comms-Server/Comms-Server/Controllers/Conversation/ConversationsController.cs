using System.Security.Claims;
using Comms_Server.DTOs.Conversation;
using Comms_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comms_Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class ConversationsController : ControllerBase
	{
		private readonly IConversationService _conversationService;
		private readonly IMessageService _messageService;

		public ConversationsController(IConversationService conversationService, IMessageService messageService)
		{
			_conversationService = conversationService;
			_messageService = messageService;
		}

		[HttpGet]
		public async Task<IActionResult> GetConversations()
		{
			if (!TryGetUserId(out var userId)) return Unauthorized();
			var conversations = await _conversationService.GetUserConversationsAsync(userId);
			return Ok(conversations);
		}

		[HttpPost]
		public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
		{
			if (!TryGetUserId(out var userId)) return Unauthorized();
			var conversation = await _conversationService.CreateConversationAsync(request.Name, request.MemberIds, userId);
			if (conversation is null)
			{
				return BadRequest("Failed to create conversation.");
			}
			return Ok(conversation);
		}

		[HttpGet("{conversationId}/messages")]
		public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
		{
			if (!TryGetUserId(out var userId)) return Unauthorized();
			if (!await _conversationService.IsUserMemberAsync(conversationId, userId))
			{
				return Forbid();
			}

			var messages = await _messageService.GetMessagesAsync(conversationId, skip, take);
			return Ok(messages);
		}

		bool TryGetUserId(out Guid userId)
		{
			var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			return Guid.TryParse(value, out userId);
		}
	}
}
