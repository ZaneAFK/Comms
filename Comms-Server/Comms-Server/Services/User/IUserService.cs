using Comms_Server.Database.Models;
using Comms_Server.DTOs;
using Comms_Server.Shared;

namespace Comms_Server.Services
{
	public interface IUserService
	{
		Task<Result<User>> RegisterUserAsync(string username, string email, string password);
		Task<Result<User>> LoginAsync(string email, string password);
		Task<User?> GetByIdAsync(Guid id);
		Task<IEnumerable<UserSearchDto>> SearchUsersAsync(string username);
	}
}
