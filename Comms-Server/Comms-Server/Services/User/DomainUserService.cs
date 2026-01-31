using Comms_Server.Database;
using Comms_Server.Database.Models.User;

namespace Comms_Server.Services.User
{
	public class DomainUserService : Service, IDomainUserService
	{
		public DomainUserService(IFactory factory) : base(factory)
		{
		}

		public async Task<DomainUser?> CreateDomainUserForSecurityUserAsync(SecurityUser securityUser)
		{
			if (securityUser == null)
			{
				return null;
			}

			var domainUserExists = await Factory.ExistsAsync<DomainUser>(u => u.SecurityUserId == securityUser.Id);
			if (domainUserExists)
			{
				return null;
			}

			var domainUser = Factory.New<DomainUser>();
			domainUser.SecurityUserId = securityUser.Id;
			domainUser.Username = securityUser.UserName ?? "";

			await Factory.SaveAsync();

			return domainUser;
		}
	}
}
