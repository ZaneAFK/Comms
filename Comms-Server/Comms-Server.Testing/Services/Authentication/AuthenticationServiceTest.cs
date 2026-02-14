using Comms_Server.Database.Models.User;
using Comms_Server.Services.Authentication;
using Comms_Server.Services.User;
using Comms_Server.Shared.Results;
using Comms_Server.Testing.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Services.Authentication
{
	[TestFixture]
	public class AuthenticationServiceTest : TransactionalTest
	{
		public required AuthenticationService AuthenticationService;

		[SetUp]
		public override async Task Setup()
		{
			await base.Setup();

			AuthenticationService = (AuthenticationService)_provider.GetRequiredService<IAuthenticationService>();
		}

		[Test]
		public async Task TestRegisterUserAsync_Succeeds()
		{
			// Act
			var result = await AuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsNotNull(result, "Registration of new user should succeed.");
			await AssertAmountOfDomainSecurityUsersSaved(1);
		}

		[Test]
		public async Task TestRegisterUserAsync_CannotRegisterSecurityUser_RollsBackAndReturnsNull()
		{
			// Arrange
			var mockSecurityUserService = new Mock<ISecurityUserService>();
			mockSecurityUserService.Setup(x => x.RegisterSecurityUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new SecurityUserRegistrationResult(IdentityResult.Failed(new IdentityError { Description = "Registration failed" }), null));

			var failingAuthenticationService = new AuthenticationService(Factory, mockSecurityUserService.Object, _provider.GetRequiredService<IDomainUserService>());

			// Act
			var result = await failingAuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsNull(result, "Registration should fail when security user cannot be created.");
			await AssertAmountOfDomainSecurityUsersSaved(0);
		}

		[Test]
		public async Task TestRegisterUserAsync_CannotCreateDomainUser_RollsBackAndReturnsNull()
		{
			// Arrange
			var mockDomainUserService = new Mock<IDomainUserService>();
			mockDomainUserService.Setup(x => x.CreateDomainUserForSecurityUserAsync(It.IsAny<SecurityUser>()))
				.ReturnsAsync((DomainUser?)null);

			var failingAuthenticationService = new AuthenticationService(Factory, _provider.GetRequiredService<ISecurityUserService>(), mockDomainUserService.Object);

			// Act
			var result = await failingAuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsNull(result, "Registration should fail when domain user cannot be created.");
			await AssertAmountOfDomainSecurityUsersSaved(0);
		}

		public async Task TestRegisterUserAsync_UserWithExistingEmail_RollsBackAndReturnsNull()
		{
			// Arrange
			var firstResult = await AuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");
			Assert.IsNotNull(firstResult, "Precondition: First registration should succeed");

			// Act
			var secondResult = await AuthenticationService.RegisterUserAsync("AnotherUser", "testuser@hotmail.com", "someweirdpassword123");

			// Assert
			Assert.IsNull(secondResult, "Registration should fail due to duplicate email.");
			await AssertAmountOfDomainSecurityUsersSaved(1);
		}
	}
}
