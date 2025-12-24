using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Comms_Server.Database.DbContext
{
	public class CommsDbContextFactory : IDesignTimeDbContextFactory<CommsDbContext>
	{
		public CommsDbContext CreateDbContext(string[] args)
		{
			// Load configuration
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.Development.json", optional: false)
				.Build();

			var optionsBuilder = new DbContextOptionsBuilder<CommsDbContext>();
			optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

			return new CommsDbContext(optionsBuilder.Options);
		}
	}
}
