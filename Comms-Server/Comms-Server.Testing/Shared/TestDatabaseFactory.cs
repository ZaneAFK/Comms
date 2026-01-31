using Comms_Server.Database.DbContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Comms_Server.Testing.Shared
{
	public static class TestDatabaseFactory
	{
		public static async Task<(ServiceProvider provider, SqliteConnection connection)> CreateAsync()
		{
			var connection = new SqliteConnection("DataSource=:memory:");
			connection.Open();

			var services = new ServiceCollection();

			// Override the DbContext to use SQLite in-memory database
			services.AddDbContext<CommsDbContext>(options =>
				options.UseSqlite(connection));

			services.AddCommsServices();

			var provider = services.BuildServiceProvider();

			var dbContext = provider.GetRequiredService<CommsDbContext>();
			dbContext.Database.EnsureCreated();

			await Database.Database.SeedRolesAsync(provider);

			return (provider, connection);
		}
	}
}
