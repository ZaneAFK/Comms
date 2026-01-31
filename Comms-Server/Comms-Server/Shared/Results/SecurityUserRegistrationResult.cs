using Comms_Server.Database.Models.User;
using Microsoft.AspNetCore.Identity;

namespace Comms_Server.Shared.Results
{
	public sealed class SecurityUserRegistrationResult
	{
		public bool Succeeded => IdentityResult.Succeeded;
		public IdentityResult IdentityResult { get; }
		public SecurityUser? SecurityUser { get; }

		public SecurityUserRegistrationResult(IdentityResult identityResult, SecurityUser? securityUser)
		{
			IdentityResult = identityResult;
			SecurityUser = securityUser;
		}
	}
}
