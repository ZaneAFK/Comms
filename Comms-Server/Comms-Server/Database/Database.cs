using Microsoft.AspNetCore.Identity;

namespace Comms_Server.Database
{
	public static class Database
	{
		public static string GetConnectionString()
		{
			return
				$"Host={Environment.GetEnvironmentVariable("DATABASE_HOST")};" +
				$"Database={Environment.GetEnvironmentVariable("DATABASE_NAME")};" +
				$"Username={Environment.GetEnvironmentVariable("DATABASE_USER")};" +
				$"Password={Environment.GetEnvironmentVariable("DATABASE_PASSWORD")};";
		}

		public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
		{
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

			var roles = new[] { "User", "Admin" };

			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					await roleManager.CreateAsync(new IdentityRole<Guid>(role));
				}
			}
		}
	}
}
