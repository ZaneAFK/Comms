using Comms_Server.Database.Models.User;

namespace Comms_Server.Services.User
{
	public interface IDomainUserService
	{
		Task<DomainUser?> CreateDomainUserForSecurityUserAsync(SecurityUser securityUser);
	}
}
