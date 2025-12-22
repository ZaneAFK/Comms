using Comms_Server.Database.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Database
{
	public class CommsDbContext : IdentityDbContext<SecurityUser, IdentityRole<Guid>, Guid>
	{
		public CommsDbContext(DbContextOptions<CommsDbContext> options)
			: base(options)
		{
		}

		public DbSet<DomainUser> DomainUsers => Set<DomainUser>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<DomainUser>()
				.HasOne(u => u.SecurityUser)
				.WithOne()
				.HasForeignKey<DomainUser>(u => u.SecurityUserId)
				.IsRequired();
		}
	}
}
