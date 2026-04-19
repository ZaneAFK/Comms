using Comms_Server.Database;
using Comms_Server.DTOs;

namespace Comms_Server.Services
{
	public class AuthenticationService : Service, IAuthenticationService
	{
		private readonly IUserService _userService;
		private readonly IJwtService _jwtService;

		public AuthenticationService(IFactory factory, IUserService userService, IJwtService jwtService) : base(factory)
		{
			_userService = userService;
			_jwtService = jwtService;
		}

		public async Task<RegisterUserResponse> RegisterUserAsync(string username, string email, string password)
		{
			var result = await _userService.RegisterUserAsync(username, email, password);

			if (!result.Succeeded)
			{
				return new RegisterUserResponse
				{
					Succeeded = false,
					Error = string.Join(" ", result.Errors)
				};
			}

			return new RegisterUserResponse { Succeeded = true };
		}

		public async Task<LoginUserResponse> LoginAsync(string email, string password)
		{
			var result = await _userService.LoginAsync(email, password);

			if (!result.Succeeded)
			{
				return new LoginUserResponse
				{
					Succeeded = false,
					Error = string.Join(" ", result.Errors)
				};
			}

			var user = result.Value!;
			var token = _jwtService.GenerateToken(user);

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
