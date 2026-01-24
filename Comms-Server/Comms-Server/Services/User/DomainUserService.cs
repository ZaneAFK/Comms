using Comms_Server.Database;
using Comms_Server.Database.Models.User;

namespace Comms_Server.Services.User
{
	public class DomainUserService : Service, IDomainUserService
	{
		public DomainUserService(IFactory factory) : base(factory)
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
