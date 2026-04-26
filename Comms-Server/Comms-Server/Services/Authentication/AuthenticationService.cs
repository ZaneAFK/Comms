using Comms_Server.Database;
using Comms_Server.DTOs;

namespace Comms_Server.Services
{
	public class AuthenticationService : Service<AuthenticationService>, IAuthenticationService
	{
		private readonly IUserService _userService;
		private readonly IJwtService _jwtService;

		public AuthenticationService(IFactory factory, ILogger<AuthenticationService> logger, IUserService userService, IJwtService jwtService) : base(factory, logger)
		{
			_userService = userService;
			_jwtService = jwtService;
		}

		public async Task<RegisterUserResponse> RegisterUserAsync(string username, string email, string password)
		{
			Logger.LogInformation("Registering user '{Username}' with email '{Email}'", username, email);

			var result = await _userService.RegisterUserAsync(username, email, password);

			if (!result.Succeeded)
			{
				Logger.LogWarning("Registration failed for '{Username}' ({Email}): {Errors}", username, email, string.Join(" ", result.Errors));
				return new RegisterUserResponse
				{
					Succeeded = false,
					Error = string.Join(" ", result.Errors)
				};
			}

			Logger.LogInformation("User '{Username}' registered successfully", username);
			return new RegisterUserResponse { Succeeded = true };
		}

		public async Task<LoginUserResponse> LoginAsync(string email, string password)
		{
			Logger.LogInformation("Login attempt for email '{Email}'", email);

			var result = await _userService.LoginAsync(email, password);

			if (!result.Succeeded)
			{
				Logger.LogInformation("Login failed for email '{Email}': {Errors}", email, string.Join(" ", result.Errors));
				return new LoginUserResponse
				{
					Succeeded = false,
					Error = string.Join(" ", result.Errors)
				};
			}

			var user = result.Value!;
			var token = _jwtService.GenerateToken(user);

			Logger.LogInformation("User '{Email}' authenticated successfully", email);
			return new LoginUserResponse
			{
				Succeeded = true,
				Token = token,
				User = new UserDto
				{
					Username = user.UserName!,
					Email = user.Email!
				}
			};
		}
	}
}
