using Comms_Server.Database.Models.User;
using Comms_Server.Services.User;
using Comms_Server.Testing.Shared;
using NUnit.Framework;

namespace Comms_Server.Testing.Services.User
{
	[TestFixture]
	public class DomainUserServiceTest : TransactionalTest
	{
		public required DomainUserService DomainUserService;

		public override async Task Setup()
		{
			await base.Setup();

			DomainUserService = new DomainUserService(Factory);
		}

		[Test]
		public async Task TestCreateDomainUserForSecurityUser_CreatesDomainUser()
		{
			// Arrange
			var result = await AuthenticationService.RegisterSecurityUserAsync("testuser", "test@gmail.com", "testpassword");
			var securityUser = result.SecurityUser ?? throw new AssertionException("SecurityUser should not be null");

			// Act
			var domainUser = await DomainUserService.CreateDomainUserForSecurityUserAsync(securityUser) ?? throw new AssertionException("DomainUser should not be null");

			// Assert
			Assert.IsNotNull(domainUser);
			Assert.IsNotNull(Factory.GetAsync<DomainUser>(domainUser.Id));
		}

		[Test]
		public async Task TestCreateDomainUserForSecurityUser_CreatingDuplicateDomainUser_ReturnsNull()
		{
			// Arrange
			var result = await AuthenticationService.RegisterSecurityUserAsync("testuser", "test@gmail.com", "testpassword");
			var securityUser = result.SecurityUser ?? throw new AssertionException("SecurityUser should not be null");

			await DomainUserService.CreateDomainUserForSecurityUserAsync(securityUser);

			// Act
			var domainUser2 = await DomainUserService.CreateDomainUserForSecurityUserAsync(securityUser);

			// Assert
			Assert.Null(domainUser2);
			Assert.IsNotNull(((List<DomainUser>)await Factory.GetAllAsync<DomainUser>()).Count() == 1);
		}
	}
}
