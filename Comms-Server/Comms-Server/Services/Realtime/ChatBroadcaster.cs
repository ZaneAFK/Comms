using Comms_Server.DTOs.Message;
using Comms_Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Comms_Server.Services
{
	// Delivers realtime events straight to the recipient users' connections via
	// SignalR's built-in user targeting (Clients.Users), instead of tracking
	// per-user connections/groups ourselves. This works out of the box because
	// ChatHub authenticates connections with a NameIdentifier claim, which is
	// exactly what the default IUserIdProvider keys connections by, and it stays
	// correct if the app is ever scaled out behind a SignalR backplane.
	public class ChatBroadcaster : IChatBroadcaster
	{
		private readonly IHubContext<ChatHub> _hubContext;

		public ChatBroadcaster(IHubContext<ChatHub> hubContext)
		{
			_hubContext = hubContext;
		}

		public Task SendMessageAsync(IEnumerable<Guid> memberIds, MessageDto message)
		{
			return _hubContext.Clients.Users(ToUserIds(memberIds)).SendAsync("ReceiveMessage", message);
		}

		public Task NotifyTypingAsync(Guid conversationId, IEnumerable<Guid> memberIds, Guid userId, string username)
		{
			return _hubContext.Clients.Users(ToUserIds(memberIds, excluding: userId))
				.SendAsync("UserTyping", new { ConversationId = conversationId, UserId = userId, Username = username });
		}

		public Task NotifyStoppedTypingAsync(Guid conversationId, IEnumerable<Guid> memberIds, Guid userId)
		{
			return _hubContext.Clients.Users(ToUserIds(memberIds, excluding: userId))
				.SendAsync("UserStoppedTyping", new { ConversationId = conversationId, UserId = userId });
		}

		private static IReadOnlyList<string> ToUserIds(IEnumerable<Guid> memberIds, Guid? excluding = null)
		{
			return memberIds
				.Where(id => id != excluding)
				.Select(id => id.ToString())
				.ToList();
		}
	}
}
