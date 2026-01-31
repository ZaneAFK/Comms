using Comms_Server.Shared.Results;

namespace Comms_Server.Services.Authentication
{
	public interface IAuthenticationService
	{
		Task<SecurityUserRegistrationResult> RegisterSecurityUserAsync(string username, string email, string password);
	}
}
