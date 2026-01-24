using Comms_Server.Database.Models.User;
using Microsoft.AspNetCore.Identity;

namespace Comms_Server.Services.Authentication
{
	public class AuthManager : IAuthManager
	{
		private readonly UserManager<SecurityUser> _userManager;

		public AuthManager(UserManager<SecurityUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IdentityResult> CreateAsync(SecurityUser user, string password)
		{
			return await _userManager.CreateAsync(user, password);
		}
	}
}
