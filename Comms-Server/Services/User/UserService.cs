using Comms_Server.Database;
using Comms_Server.Database.Models.User;

namespace Comms_Server.Services.User
{
	public class UserService : Service
	{
		public UserService(IFactory factory) : base(factory)
		{
		}

		public async Task<DomainUser?> GetUserAsync(Guid id)
		{
			return await Factory.GetAsync<DomainUser>(id);
		}

		public async Task AddUserAsync(DomainUser user)
		{
			await Factory.AddAsync(user);
		}
	}
}
