using Comms_Server.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Database
{
	public class CommsDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
	{
		public CommsDbContext(DbContextOptions<CommsDbContext> options)
			: base(options)
		{
		}
	}
}
