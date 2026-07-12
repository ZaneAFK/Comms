using Comms_Server.DTOs.Message;
using Comms_Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Comms_Server.Services
{
	public class ChatHub : IChatHub
	{
		private readonly IHubContext<ChatSignalRHub> _hubContext;
		private readonly IUserConnectionTracker _userConnectionTracker;

		public ChatHub(IHubContext<ChatSignalRHub> hubContext, IUserConnectionTracker userConnectionTracker)
		{
			_hubContext = hubContext;
			_userConnectionTracker = userConnectionTracker;
		}

		public async Task RegisterConnectionAsync(Guid userId, string connectionId, IEnumerable<Guid> conversationIds)
		{
			_userConnectionTracker.AddConnection(userId, connectionId);

			foreach (var conversationId in conversationIds)
			{
				await _hubContext.Groups.AddToGroupAsync(connectionId, GroupName(conversationId));
			}
		}

		public Task UnregisterConnectionAsync(Guid userId, string connectionId)
		{
			_userConnectionTracker.RemoveConnection(userId, connectionId);
			return Task.CompletedTask;
		}

		public async Task AddUsersToConversationAsync(IEnumerable<Guid> userIds, Guid conversationId)
		{
			var groupName = GroupName(conversationId);

			foreach (var userId in userIds)
			{
				foreach (var connectionId in _userConnectionTracker.GetConnections(userId))
				{
					await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
				}
			}
		}

		public Task SendMessageAsync(Guid conversationId, MessageDto message)
		{
			return _hubContext.Clients.Group(GroupName(conversationId)).SendAsync("ReceiveMessage", message);
		}

		public Task NotifyTypingAsync(Guid conversationId, string excludingConnectionId, Guid userId, string username)
		{
			return _hubContext.Clients.GroupExcept(GroupName(conversationId), [excludingConnectionId])
				.SendAsync("UserTyping", new { ConversationId = conversationId, UserId = userId, Username = username });
		}

		public Task NotifyStoppedTypingAsync(Guid conversationId, string excludingConnectionId, Guid userId)
		{
			return _hubContext.Clients.GroupExcept(GroupName(conversationId), [excludingConnectionId])
				.SendAsync("UserStoppedTyping", new { ConversationId = conversationId, UserId = userId });
		}

		private static string GroupName(Guid conversationId) => conversationId.ToString();
	}
}
