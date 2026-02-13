using Comms_Server.Database;
using Comms_Server.DTOs;
using Comms_Server.Services.Authentication;

namespace Comms_Server.Services.User
{
	public class UserService : Service, IUserService
	{
		private readonly IAuthenticationService _authenticationService;
		private readonly IDomainUserService _domainUserService;

		public UserService(IFactory factory, IAuthenticationService authenticationService, IDomainUserService domainUserService) : base(factory)
		{
			_authenticationService = authenticationService;
			_domainUserService = domainUserService;
		}

		public async Task<RegisterUserResponse?> RegisterUserAsync(string username, string email, string password)
		{
			using var transaction = await Factory.BeginTransactionAsync();

			try
			{
				var result = await _authenticationService.RegisterSecurityUserAsync(username, email, password);

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
