using Comms_Server.Database;

namespace Comms_Server.Services
{
	public abstract class Service
	{
		protected readonly IFactory Factory;
		protected Service(IFactory factory) { Factory = factory; }
	}
}
