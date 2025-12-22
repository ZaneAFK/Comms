using Comms_Server.Database;
using Comms_Server.Database.DbContext;
using Comms_Server.Services.Authentication;
using Comms_Server.Services.User;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server
{
	public static class ServiceExtensions
	{
		public static IServiceCollection AddCommsDb(this IServiceCollection services, string? connectionString)
		{
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
