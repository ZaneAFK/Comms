using Comms_Server.Database.Models.User;
using Microsoft.AspNetCore.Identity;

namespace Comms_Server.Services.Authentication
{
	public interface IAuthManager
	{
		Task<IdentityResult> CreateAsync(SecurityUser user, string password);
	}
}
