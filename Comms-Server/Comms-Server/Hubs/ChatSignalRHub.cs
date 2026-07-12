using System.Security.Claims;
using Comms_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Comms_Server.Hubs
{
	[Authorize]
	public class ChatSignalRHub : Hub
	{
		private readonly IMessageService _messageService;
		private readonly IConversationService _conversationService;
		private readonly IChatHub _chatHub;

		public ChatSignalRHub(IMessageService messageService, IConversationService conversationService, IChatHub chatHub)
		{
			_messageService = messageService;
			_conversationService = conversationService;
			_chatHub = chatHub;
		}

		public override async Task OnConnectedAsync()
		{
			var userId = GetUserId();
			var conversationIds = await _conversationService.GetUserConversationIdsAsync(userId);
			await _chatHub.RegisterConnectionAsync(userId, Context.ConnectionId, conversationIds);
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			await _chatHub.UnregisterConnectionAsync(GetUserId(), Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}

		public async Task SendMessage(Guid conversationId, string content)
		{
			var userId = GetUserId();
			if (!await _conversationService.IsUserMemberAsync(conversationId, userId))
			{
				return;
			}

			var message = await _messageService.CreateMessageAsync(conversationId, userId, content);
			await _chatHub.SendMessageAsync(conversationId, message);
		}

		public async Task StartTyping(Guid conversationId)
		{
			var userId = GetUserId();
			var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
			await _chatHub.NotifyTypingAsync(conversationId, Context.ConnectionId, userId, username);
		}

		public async Task StopTyping(Guid conversationId)
		{
			var userId = GetUserId();
			await _chatHub.NotifyStoppedTypingAsync(conversationId, Context.ConnectionId, userId);
		}

		private Guid GetUserId()
		{
			var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			return Guid.TryParse(value, out var id) ? id : Guid.Empty;
		}
	}
}
