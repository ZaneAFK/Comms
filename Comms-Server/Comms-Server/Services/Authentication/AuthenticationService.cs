using Comms_Server.Database;
using Comms_Server.DTOs;
using Comms_Server.Services.User;

namespace Comms_Server.Services.Authentication
{
	public class AuthenticationService : Service, IAuthenticationService
	{
		private readonly ISecurityUserService _securityUserService;
		private readonly IDomainUserService _domainUserService;

		public AuthenticationService(IFactory factory, ISecurityUserService securityUserService, IDomainUserService domainUserService) : base(factory)
		{
			_securityUserService = securityUserService;
			_domainUserService = domainUserService;
		}

		public async Task<RegisterUserResponse?> RegisterUserAsync(string username, string email, string password)
		{
			using var transaction = await Factory.BeginTransactionAsync();

			try
			{
				var result = await _securityUserService.RegisterSecurityUserAsync(username, email, password);

				if (!result.Succeeded)
				{
					return null;
				}

				var domainUser = await _domainUserService.CreateDomainUserForSecurityUserAsync(result.SecurityUser!);

				if (domainUser is null)
				{
					return null;
				}

				transaction.Commit();

				return new RegisterUserResponse
				{
					UserId = domainUser.Id,
					Username = domainUser.Username,
					Email = result.SecurityUser!.Email!
				};
			}
			catch
			{
				return null;
			}
		}
	}
}
