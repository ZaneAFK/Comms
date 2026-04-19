using System.Text;
using Comms_Server.Database;
using Comms_Server.Database.Models;
using Comms_Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

		public static IServiceCollection AddCommsServices(this IServiceCollection services, IConfiguration configuration)
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

			// JWT authentication
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = configuration["Jwt:Issuer"],
					ValidAudience = configuration["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
				};
			});

			// Application services
			services.AddScoped<IAuthenticationService, AuthenticationService>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IJwtService, JwtService>();

			// Logging
			services.AddLogging();

			return services;
		}
	}
}
