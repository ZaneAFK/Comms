using Comms_Server.Database.Models.User;
using Comms_Server.Services.User;
using Comms_Server.Testing.Shared;
using NUnit.Framework;

namespace Comms_Server.Testing.Services.User
{
	[TestFixture]
	public class DomainUserServiceTest : TransactionalTest
	{
		private DomainUserService domainUserService;

		public override async Task Setup()
		{
			await base.Setup();

			domainUserService = new DomainUserService(Factory);
		}

		[Test]
		public async Task TestCreateDomainUserForSecurityUser_CreatesDomainUser()
		{
			// Arrange
			var result = await AuthenticationService.RegisterSecurityUserAsync("testuser", "test@gmail.com", "testpassword");
			Assert.IsNotNull(result.SecurityUser);

			// Act
			var domainUser = await domainUserService.CreateDomainUserForSecurityUserAsync(result.SecurityUser);

			// Assert
			Assert.IsNotNull(domainUser);
			Assert.IsNotNull(Factory.GetAsync<DomainUser>(domainUser.Id));
		}

		[Test]
		public async Task TestCreateDomainUserForSecurityUser_CreatingDuplicateDomainUser_ReturnsNull()
		{
			// Arrange
			var result = await AuthenticationService.RegisterSecurityUserAsync("testuser", "test@gmail.com", "testpassword");
			Assert.IsNotNull(result.SecurityUser);

			await domainUserService.CreateDomainUserForSecurityUserAsync(result.SecurityUser);

			// Act
			var domainUser2 = await domainUserService.CreateDomainUserForSecurityUserAsync(result.SecurityUser);

			// Assert
			Assert.Null(domainUser2);
			Assert.IsNotNull(((List<DomainUser>)await Factory.GetAllAsync<DomainUser>()).Count() == 1);
		}
	}
}
