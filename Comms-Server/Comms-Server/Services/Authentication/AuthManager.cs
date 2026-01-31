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

		public async Task<IdentityResult> RegisterSecurityUserAsync(SecurityUser securityUser, string password)
		{
			var result = await _userManager.CreateAsync(securityUser, password);

			if (result.Succeeded)
			{
				await _userManager.AddToRolesAsync(securityUser, new List<string> { "User" });
			}

			return result;
		}
	}
}
