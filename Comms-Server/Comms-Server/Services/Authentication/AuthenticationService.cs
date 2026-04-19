using Comms_Server.Database;
using Comms_Server.DTOs;

namespace Comms_Server.Services
{
	public class AuthenticationService : Service, IAuthenticationService
	{
		private readonly IUserService _userService;

		public AuthenticationService(IFactory factory, IUserService userService) : base(factory)
		{
			_userService = userService;
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
	}
}
