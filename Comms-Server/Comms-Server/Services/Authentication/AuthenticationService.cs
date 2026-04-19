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

		public async Task<RegisterUserResponse?> RegisterUserAsync(string username, string email, string password)
		{
			var result = await _userService.RegisterUserAsync(username, email, password);

			if (!result.Succeeded)
			{
				return null;
			}

			var user = result.Value!;

			return new RegisterUserResponse
			{
				UserId = user.Id,
				Username = user.UserName!,
				Email = user.Email!
			};
		}
	}
}
