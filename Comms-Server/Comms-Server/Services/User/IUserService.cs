using Comms_Server.DTOs;

namespace Comms_Server.Services.User
{
	public interface IUserService
	{
		Task<RegisterUserResponse?> RegisterUserAsync(string username, string email, string password);
	}
}
