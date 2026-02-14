using Comms_Server.Database;
using Comms_Server.Database.Models.User;
using Comms_Server.Shared.Results;

namespace Comms_Server.Services.Authentication
{
	public class SecurityUserService : Service, ISecurityUserService
	{
		private readonly IAuthManager _authManager;

		public SecurityUserService(IFactory factory, IAuthManager authManager) : base(factory)
		{
			_authManager = authManager;
		}

		/// <summary>
		/// Registers a new security user with the provided username, email and password.
		/// It is NOT required to call SaveChangesAsync after this method, as it is handled internally.
		/// </summary>
		public async Task<SecurityUserRegistrationResult> RegisterSecurityUserAsync(string username, string email, string password)
		{
			try
			{
				var securityUser = new SecurityUser
				{
					UserName = username,
					Email = email
				};

				var result = await _authManager.RegisterSecurityUserAsync(securityUser, password);

				if (!result.Succeeded)
				{
					return new SecurityUserRegistrationResult(result, null);
				}

				return new SecurityUserRegistrationResult(result, securityUser);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to create and register SecurityUser instance.", ex);
			}
		}
	}
}
