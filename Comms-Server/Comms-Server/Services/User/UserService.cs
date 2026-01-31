using Comms_Server.Database;

namespace Comms_Server.Services.User
{
	public class UserService : Service, IUserService
	{
		public UserService(IFactory factory) : base(factory)
		{
		}
	}
}
