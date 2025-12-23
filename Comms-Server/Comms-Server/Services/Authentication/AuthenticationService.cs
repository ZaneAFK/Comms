using Comms_Server.Database;

namespace Comms_Server.Services.Authentication
{
	public class AuthenticationService : Service, IAuthenticationService
	{
		public AuthenticationService(IFactory factory) : base(factory)
		{
		}
	}
}