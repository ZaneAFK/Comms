using Comms_Server.Shared.Results;
using UserModel = Comms_Server.Database.Models.User.User;

namespace Comms_Server.Services.User
{
	public interface IUserService
	{
		Task<Result<UserModel>> RegisterUserAsync(string username, string email, string password);
		Task<UserModel?> GetByIdAsync(Guid id);
	}
}
