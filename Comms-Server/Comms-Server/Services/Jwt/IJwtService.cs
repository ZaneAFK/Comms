using Comms_Server.Database.Models;

namespace Comms_Server.Services
{
	public interface IJwtService
	{
		string GenerateToken(User user);
	}
}
