using Comms_Server.Database;
using Comms_Server.Database.Models.User;
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

		protected async Task AssertAmountOfDomainUsersSaved(int expectedAmount)
		{
			var domainUsers = (List<DomainUser>)await Factory.GetAllAsync<DomainUser>();
			Assert.AreEqual(domainUsers.Count, expectedAmount,
				$"Expected {expectedAmount} DomainUser(s) in the database, but found {domainUsers.Count}.");
		}

		protected async Task AssertAmountOfSecurityUsersSaved(int expectedAmount)
		{
			var securityUsers = (List<SecurityUser>)await Factory.GetAllAsync<SecurityUser>();
			Assert.AreEqual(securityUsers.Count, expectedAmount,
				$"Expected {expectedAmount} SecurityUser(s) in the database, but found {securityUsers.Count}.");
		}

		protected async Task AssertAmountOfDomainSecurityUsersSaved(int expectedAmount)
		{
			await AssertAmountOfDomainUsersSaved(expectedAmount);
			await AssertAmountOfSecurityUsersSaved(expectedAmount);
		}

		public void Dispose()
		{
			_provider?.Dispose();
			_connection?.Dispose();
		}
	}
}
