using Comms_Server.Database;
using Comms_Server.Database.DbContext;
using Comms_Server.Database.Models.User;
using Comms_Server.Services.Authentication;
using Comms_Server.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server
{
	public static class ServiceExtensions
	{
		public static IServiceCollection AttachCommsDatabase(this IServiceCollection services)
		{
			services.AddDbContext<CommsDbContext>(options =>
				options.UseNpgsql(Database.Database.GetConnectionString())
			);

			return services;
		}

		public static IServiceCollection AddCommsServices(this IServiceCollection services)
		{
			// Core database service
			services.AddScoped<IFactory, Factory>();

			// Identity services
			services.AddIdentity<SecurityUser, IdentityRole<Guid>>(options =>
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
			services.AddScoped<IDomainUserService, DomainUserService>();
			services.AddScoped<ISecurityUserService, SecurityUserService>();
			services.AddScoped<IAuthManager, AuthManager>();

			// Logging
			services.AddLogging();

			return services;
		}
	}
}
