namespace Comms_Server.Services
{
	public interface IUserConnectionTracker
	{
		void AddConnection(Guid userId, string connectionId);
		void RemoveConnection(Guid userId, string connectionId);
		IReadOnlyCollection<string> GetConnections(Guid userId);
	}
}
