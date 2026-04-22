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

		public ChatHub(IMessageService messageService, IConversationService conversationService)
		{
			_messageService = messageService;
			_conversationService = conversationService;
		}

		public override async Task OnConnectedAsync()
		{
			var userId = GetUserId();
			var conversations = await _conversationService.GetUserConversationsAsync(userId);
			foreach (var conversation in conversations)
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, conversation.Id.ToString());
			}
			await base.OnConnectedAsync();
		}

		public async Task SendMessage(Guid conversationId, string content)
		{
			var userId = GetUserId();
			if (!await _conversationService.IsUserMemberAsync(conversationId, userId))
			{
				return;
			}

			var message = await _messageService.CreateMessageAsync(conversationId, userId, content);
			await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", message);
		}

		public async Task StartTyping(Guid conversationId)
		{
			var userId = GetUserId();
			var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
			await Clients.OthersInGroup(conversationId.ToString())
				.SendAsync("UserTyping", new { ConversationId = conversationId, UserId = userId, Username = username });
		}

		public async Task StopTyping(Guid conversationId)
		{
			var userId = GetUserId();
			await Clients.OthersInGroup(conversationId.ToString())
				.SendAsync("UserStoppedTyping", new { ConversationId = conversationId, UserId = userId });
		}

		public async Task JoinConversation(Guid conversationId)
		{
			var userId = GetUserId();
			if (await _conversationService.IsUserMemberAsync(conversationId, userId))
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
			}
		}

		private Guid GetUserId()
		{
			var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			return Guid.TryParse(value, out var id) ? id : Guid.Empty;
		}
	}
}
