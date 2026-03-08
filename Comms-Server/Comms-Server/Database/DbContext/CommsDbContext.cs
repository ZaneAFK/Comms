using Comms_Server.Database.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Database.DbContext
{
	public class CommsDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
	{
		public CommsDbContext(DbContextOptions<CommsDbContext> options)
			: base(options)
		{
		}
	}
}
