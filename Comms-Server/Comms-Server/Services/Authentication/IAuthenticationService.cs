using Comms_Server.DTOs;

namespace Comms_Server.Services
{
	public interface IAuthenticationService
	{
		Task<RegisterUserResponse> RegisterUserAsync(string username, string email, string password);
	}
}
