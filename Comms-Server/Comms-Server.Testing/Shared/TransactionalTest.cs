using Comms_Server.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Comms_Server.Testing.Shared
{
	public abstract class TransactionalTest : IDisposable
	{
		private SqliteConnection? _connection = null;
		protected ServiceProvider _provider = null!;

		public required CommsDbContext Db;

		[SetUp]
		public virtual async Task Setup()
		{
			(_provider, _connection) = await TestDatabaseFactory.CreateAsync();

			Db = _provider.GetRequiredService<CommsDbContext>();
		}

		protected async Task AssertAmountOfUsersSaved(int expectedAmount)
		{
			var userCount = await Db.Users.CountAsync();
			Assert.AreEqual(expectedAmount, userCount,
				$"Expected {expectedAmount} User(s) in the database, but found {userCount}.");
		}

		public void Dispose()
		{
			_provider?.Dispose();
			_connection?.Dispose();
		}
	}
}
