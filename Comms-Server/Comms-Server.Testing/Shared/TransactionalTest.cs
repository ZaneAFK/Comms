using Comms_Server.Database;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Comms_Server.Testing.Shared
{
	public abstract class TransactionalTest : IDisposable
	{
		private SqliteConnection? _connection = null;
		protected ServiceProvider _provider = null!;

		public required IFactory Factory;

		[SetUp]
		public virtual async Task Setup()
		{
			(_provider, _connection) = await TestDatabaseFactory.CreateAsync();

			Factory = _provider.GetRequiredService<IFactory>();
		}

		public void Dispose()
		{
			_provider?.Dispose();
			_connection?.Dispose();
		}
	}
}
