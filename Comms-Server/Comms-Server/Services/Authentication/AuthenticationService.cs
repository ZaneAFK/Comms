using Comms_Server.DTOs;

namespace Comms_Server.Services
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly ILogger<AuthenticationService> _logger;
		private readonly IUserService _userService;
		private readonly IJwtService _jwtService;

		public AuthenticationService(ILogger<AuthenticationService> logger, IUserService userService, IJwtService jwtService)
		{
			_logger = logger;
			_userService = userService;
			_jwtService = jwtService;
		}

		public async Task<RegisterUserResponse> RegisterUserAsync(string username, string email, string password)
		{
			_logger.LogDebug("Registering user '{Username}' with email '{Email}'", username, email);

			var result = await _userService.RegisterUserAsync(username, email, password);

			if (!result.Succeeded)
			{
				_logger.LogWarning("Registration failed for '{Username}' ({Email}): {Errors}", username, email, string.Join(" ", result.Errors));
				return new RegisterUserResponse
				{
					Succeeded = false,
					Error = string.Join(" ", result.Errors)
				};
			}

			_logger.LogDebug("User '{Username}' registered successfully", username);
			return new RegisterUserResponse { Succeeded = true };
		}

		public async Task<LoginUserResponse> LoginAsync(string email, string password)
		{
			_logger.LogDebug("Attempting login for email '{Email}'", email);

			var result = await _userService.LoginAsync(email, password);

			if (!result.Succeeded)
			{
				_logger.LogWarning("Login failed for email '{Email}': {Errors}", email, string.Join(" ", result.Errors));
				return new LoginUserResponse
				{
					Succeeded = false,
					Error = string.Join(" ", result.Errors)
				};
			}

			var user = result.Value!;
			var token = _jwtService.GenerateToken(user);

			_logger.LogDebug("User '{Email}' authenticated successfully", email);
			return new LoginUserResponse
			{
				Succeeded = true,
				Token = token,
				User = new UserDto
				{
					Id = user.Id,
					Username = user.UserName!,
					Email = user.Email!
				}
			};
		}
	}
}
