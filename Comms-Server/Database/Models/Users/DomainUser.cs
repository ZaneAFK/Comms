namespace Comms_Server.Database.Models.Users
{
	public class DomainUser
	{
		public Guid Id { get; set; }

		public Guid SecurityUserId { get; set; }
		public SecurityUser SecurityUser { get; set; } = null!;

		public string Username { get; set; } = null!;
		public DateTime CreatedTime { get; set; } = DateTime.Now;
	}
}
