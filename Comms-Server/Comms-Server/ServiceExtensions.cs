using Comms_Server.Database;
using Comms_Server.Database.DbContext;
using Comms_Server.Services.Authentication;
using Comms_Server.Services.User;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server
{
	public static class ServiceExtensions
	{
		public static IServiceCollection AttachCommsDatabase(this IServiceCollection services)
		{
			var connectionString =
				$"Host={Environment.GetEnvironmentVariable("DATABASE_HOST")};" +
				$"Database={Environment.GetEnvironmentVariable("DATABASE_NAME")};" +
				$"Username={Environment.GetEnvironmentVariable("DATABASE_USER")};" +
				$"Password={Environment.GetEnvironmentVariable("DATABASE_PASSWORD")};";

			services.AddDbContext<CommsDbContext>(options =>
				options.UseNpgsql(connectionString)
			);

			return services;
		}

		public static IServiceCollection AddCommsServices(this IServiceCollection services)
		{
			services.AddScoped<IFactory, Factory>();

			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IAuthenticationService, AuthenticationService>();

			return services;
		}
	}
}
