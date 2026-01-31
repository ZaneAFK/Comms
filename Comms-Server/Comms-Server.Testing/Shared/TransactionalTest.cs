using Comms_Server.Database;
using Comms_Server.Services.Authentication;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Comms_Server.Testing.Shared
{
	public abstract class TransactionalTest : IDisposable
	{
		private SqliteConnection _connection;
		private ServiceProvider _provider;

		protected IFactory Factory;
		protected IAuthenticationService AuthenticationService;

		[SetUp]
		public virtual async Task Setup()
		{
			(_provider, _connection) = await TestDatabaseFactory.CreateAsync();

			Factory = _provider.GetRequiredService<IFactory>();
			AuthenticationService = _provider.GetRequiredService<IAuthenticationService>();
		}

		public void Dispose()
		{
			_provider.Dispose();
			_connection.Dispose();
		}
	}
}
