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

		public async Task<IdentityResult> RegisterSecurityUser(SecurityUser user, string password)
		{
			try
			{
				var securityUser = await _userManager.CreateAsync(user, password);
				await _userManager.AddToRolesAsync(user, new List<string> { "User" });

				return securityUser;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error registering user: {ex.Message}");
			}
		}
	}
}
