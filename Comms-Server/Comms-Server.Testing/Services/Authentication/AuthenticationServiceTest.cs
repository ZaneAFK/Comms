using Comms_Server.Database.Models;
using Comms_Server.Services;
using Comms_Server.Shared;
using Comms_Server.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Services
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
			await AssertAmountOfUsersSaved(1);
		}

		[Test]
		public async Task TestRegisterUserAsync_CannotRegisterUser_ReturnsNull()
		{
			// Arrange
			var mockUserService = new Mock<IUserService>();
			mockUserService
				.Setup(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(Result<User>.Failure(new[] { "Registration failed" }));

			var failingAuthenticationService = new AuthenticationService(Factory, mockUserService.Object);

			// Act
			var result = await failingAuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsNull(result, "Registration should fail when user service returns a failure result.");
			await AssertAmountOfUsersSaved(0);
		}

		[Test]
		public async Task TestRegisterUserAsync_UserWithExistingEmail_ReturnsNull()
		{
			// Arrange
			var firstResult = await AuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");
			Assert.IsNotNull(firstResult, "Precondition: First registration should succeed");

			// Act
			var secondResult = await AuthenticationService.RegisterUserAsync("AnotherUser", "testuser@hotmail.com", "someweirdpassword123");

			// Assert
			Assert.IsNull(secondResult, "Registration should fail due to duplicate email.");
			await AssertAmountOfUsersSaved(1);
		}
	}
}
