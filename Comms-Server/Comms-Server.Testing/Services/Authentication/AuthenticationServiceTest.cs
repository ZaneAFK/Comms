using Comms_Server.Database.Models.User;
using Comms_Server.Services.Authentication;
using Comms_Server.Testing.Shared;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Services.Authentication
{
	[TestFixture]
	public class AuthenticationServiceTest : TransactionalTest
	{
		[SetUp]
		public override async Task Setup()
		{
			await base.Setup();
		}

		[Test]
		public async Task TestRegisterSecurityUserAsync_Succeeds()
		{
			// Act
			var result = await AuthenticationService.RegisterSecurityUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");
			var securityUser = result.SecurityUser ?? throw new AssertionException("SecurityUser should not be null");

			// Assert
			Assert.IsTrue(result.Succeeded);
			Assert.IsNotNull(Factory.GetAsync<SecurityUser>(securityUser.Id));
		}

		[Test]
		public async Task TestRegisterSecurityUserAsync_Fails()
		{
			// Arrange
			var mockAuthManager = new Mock<IAuthManager>();
			mockAuthManager
				.Setup(am => am.RegisterSecurityUserAsync(It.IsAny<SecurityUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Registration failed" }));
			var failingAuthenticationService = new AuthenticationService(Factory, mockAuthManager.Object);

			// Act
			var result = await failingAuthenticationService.RegisterSecurityUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsFalse(result.Succeeded);
			Assert.IsNull(result.SecurityUser);
		}

		[Test]
		public async Task TestRegisterSecurityUserAsync_ExistingSecurityUserWithEmail_Fails()
		{
			// Arrange
			var firstResult = await AuthenticationService.RegisterSecurityUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");
			Assert.IsTrue(firstResult.Succeeded, "Precondition: First registration should succeed");

			// Act
			var result = await AuthenticationService.RegisterSecurityUserAsync("AnotherUser", "testuser@hotmail.com", "anotherpassword");

			// Assert
			Assert.IsFalse(result.Succeeded);
			var errorMessage = result.IdentityResult.Errors.FirstOrDefault()?.Description;
			Assert.AreEqual("Email 'testuser@hotmail.com' is already taken.", errorMessage);
		}
	}
}
