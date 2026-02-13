using Comms_Server.Services.Authentication;
using Comms_Server.Services.User;
using Comms_Server.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Comms_Server.Testing.Services.User
{
	[TestFixture]
	public class DomainUserServiceTest : TransactionalTest
	{
		public required IAuthenticationService AuthenticationService;
		public required DomainUserService DomainUserService;

		public override async Task Setup()
		{
			await base.Setup();

			DomainUserService = (DomainUserService)_provider.GetRequiredService<IDomainUserService>();
			AuthenticationService = _provider.GetRequiredService<IAuthenticationService>();
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
			Assert.IsNotNull(domainUser, "Domain user should have been successfully created.");
			AssertAmountOfDomainUsersSaved(1);
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
			Assert.IsNull(domainUser2, "Domain user should have failed to create due there already existing a security and domain user.");
			AssertAmountOfDomainUsersSaved(1);
		}
	}
}
