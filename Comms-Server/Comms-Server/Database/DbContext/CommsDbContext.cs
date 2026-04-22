using Comms_Server.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Database
{
	public class CommsDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
	{
		public DbSet<Conversation> Conversations { get; set; }
		public DbSet<ConversationMember> ConversationMembers { get; set; }
		public DbSet<Message> Messages { get; set; }

		public CommsDbContext(DbContextOptions<CommsDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<ConversationMember>()
				.HasKey(cm => new { cm.ConversationId, cm.UserId });

			builder.Entity<ConversationMember>()
				.HasOne(cm => cm.Conversation)
				.WithMany(c => c.Members)
				.HasForeignKey(cm => cm.ConversationId);

			builder.Entity<ConversationMember>()
				.HasOne(cm => cm.User)
				.WithMany()
				.HasForeignKey(cm => cm.UserId);

			builder.Entity<Message>()
				.HasOne(m => m.Conversation)
				.WithMany(c => c.Messages)
				.HasForeignKey(m => m.ConversationId);

			builder.Entity<Message>()
				.HasOne(m => m.Sender)
				.WithMany()
				.HasForeignKey(m => m.SenderId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
