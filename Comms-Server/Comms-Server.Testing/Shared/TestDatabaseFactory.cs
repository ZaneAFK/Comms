using Comms_Server.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Comms_Server.Testing.Shared
{
	public static class TestDatabaseFactory
	{
		public static async Task<(ServiceProvider provider, SqliteConnection connection)> CreateAsync()
		{
			var connection = new SqliteConnection("DataSource=:memory:");
			connection.Open();

			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					{ "Jwt:Key", "test-secret-key-for-unit-tests-minimum-32-chars!" },
					{ "Jwt:Issuer", "test-issuer" },
					{ "Jwt:Audience", "test-audience" },
					{ "Jwt:ExpiryMinutes", "60" }
				})
				.Build();

			var services = new ServiceCollection();

			services.AddSingleton<IConfiguration>(configuration);

			services.AddDbContext<CommsDbContext>(options =>
				options.UseSqlite(connection));

			services.AddCommsServices(configuration);

			var provider = services.BuildServiceProvider();

			var dbContext = provider.GetRequiredService<CommsDbContext>();
			dbContext.Database.EnsureCreated();

			await Database.Database.SeedRolesAsync(provider);

			return (provider, connection);
		}
	}
}
