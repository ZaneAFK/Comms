using System.Security.Claims;
using Comms_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Comms_Server.Hubs
{
	[Authorize]
	public class ChatHub : Hub
	{
		private readonly IMessageService _messageService;
		private readonly IConversationService _conversationService;
		private readonly IChatBroadcaster _chatBroadcaster;

		public ChatHub(IMessageService messageService, IConversationService conversationService, IChatBroadcaster chatBroadcaster)
		{
			_messageService = messageService;
			_conversationService = conversationService;
			_chatBroadcaster = chatBroadcaster;
		}

		public async Task SendMessage(Guid conversationId, string content)
		{
			var userId = GetUserId();
			if (!await _conversationService.IsUserMemberAsync(conversationId, userId))
			{
				return;
			}

			var message = await _messageService.CreateMessageAsync(conversationId, userId, content);
			var memberIds = await _conversationService.GetConversationMemberIdsAsync(conversationId);
			await _chatBroadcaster.SendMessageAsync(memberIds, message);
		}

		public async Task StartTyping(Guid conversationId)
		{
			var userId = GetUserId();
			var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
			var memberIds = await _conversationService.GetConversationMemberIdsAsync(conversationId);
			await _chatBroadcaster.NotifyTypingAsync(conversationId, memberIds, userId, username);
		}

		public async Task StopTyping(Guid conversationId)
		{
			var userId = GetUserId();
			var memberIds = await _conversationService.GetConversationMemberIdsAsync(conversationId);
			await _chatBroadcaster.NotifyStoppedTypingAsync(conversationId, memberIds, userId);
		}

		private Guid GetUserId()
		{
			var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			return Guid.TryParse(value, out var id) ? id : Guid.Empty;
		}
	}
}
