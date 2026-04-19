using Comms_Server.Database;
using Comms_Server.Database.Models;
using Comms_Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server
{
	public static class ServiceExtensions
	{
		public static IServiceCollection AttachCommsDatabase(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<CommsDbContext>(options =>
				options.UseNpgsql(configuration.GetConnectionString("Default"))
			);

			return services;
		}

		public static IServiceCollection AddCommsServices(this IServiceCollection services)
		{
			// Core database service
			services.AddScoped<IFactory, Factory>();

			// Identity services
			services.AddIdentity<User, IdentityRole<Guid>>(options =>
			{
				options.Password.RequireDigit = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireNonAlphanumeric = false;
				options.User.RequireUniqueEmail = true;
			})
				.AddEntityFrameworkStores<CommsDbContext>()
				.AddDefaultTokenProviders();

			// Application services
			services.AddScoped<IAuthenticationService, AuthenticationService>();
			services.AddScoped<IUserService, UserService>();

			// Logging
			services.AddLogging();

			return services;
		}
	}
}
