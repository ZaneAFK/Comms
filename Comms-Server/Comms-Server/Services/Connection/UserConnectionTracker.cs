using System.Collections.Concurrent;

namespace Comms_Server.Services
{
	// Tracks which SignalR connections belong to which user, so features outside
	// the hub (e.g. the conversations controller) can push that user into a group.
	public class UserConnectionTracker : IUserConnectionTracker
	{
		private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _connectionsByUser = new();

		public void AddConnection(Guid userId, string connectionId)
		{
			var connections = _connectionsByUser.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
			connections[connectionId] = 0;
		}

		public void RemoveConnection(Guid userId, string connectionId)
		{
			if (_connectionsByUser.TryGetValue(userId, out var connections))
			{
				connections.TryRemove(connectionId, out _);
			}
		}

		public IReadOnlyCollection<string> GetConnections(Guid userId)
		{
			return _connectionsByUser.TryGetValue(userId, out var connections) ? connections.Keys.ToArray() : [];
		}
	}
}
