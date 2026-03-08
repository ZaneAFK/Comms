using Microsoft.AspNetCore.Identity;

namespace Comms_Server.Database.Models.User
{
	public class User : IdentityUser<Guid>
	{
		public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
	}
}
