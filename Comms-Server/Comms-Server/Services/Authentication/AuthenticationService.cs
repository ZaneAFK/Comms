using Comms_Server.Database;

namespace Comms_Server.Services.Authentication
{
	public class AuthenticationService : Service, IAuthenticationService
	{
		private readonly IAuthManager _authManager;

		public AuthenticationService(IFactory factory, IAuthManager authManager) : base(factory)
		{
			_authManager = authManager;
		}
	}
}
