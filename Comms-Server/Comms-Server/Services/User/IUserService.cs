using Comms_Server.Database.Models.User;
using Comms_Server.Shared.Results;

namespace Comms_Server.Services.User
{
	public interface IUserService
	{
		Task<Result<AppUser>> RegisterUserAsync(string username, string email, string password);
		Task<AppUser?> GetByIdAsync(Guid id);
	}
}
