using Comms_Server.Database;

namespace Comms_Server.Services
{
	public abstract class Service<T>
	{
		protected readonly IFactory Factory;
		protected readonly ILogger<T> Logger;

		protected Service(IFactory factory, ILogger<T> logger)
		{
			Factory = factory;
			Logger = logger;
		}
	}
}
