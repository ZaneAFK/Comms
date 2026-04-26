using Comms_Server.Database.Models;
using Comms_Server.Services;
using Comms_Server.Shared;
using Comms_Server.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
			Assert.IsTrue(result.Succeeded, "Registration of new user should succeed.");
			Assert.IsNull(result.Error);
			await AssertAmountOfUsersSaved(1);
		}

		[Test]
		public async Task TestRegisterUserAsync_CannotRegisterUser_ReturnsFailed()
		{
			// Arrange
			var mockUserService = new Mock<IUserService>();
			mockUserService
				.Setup(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(Result<User>.Failure(new[] { "Registration failed" }));

			var failingAuthenticationService = new AuthenticationService(Factory, _provider.GetRequiredService<ILogger<AuthenticationService>>(), mockUserService.Object, _provider.GetRequiredService<IJwtService>());

			// Act
			var result = await failingAuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsFalse(result.Succeeded, "Registration should fail when user service returns a failure result.");
			Assert.IsNotNull(result.Error);
			await AssertAmountOfUsersSaved(0);
		}

		[Test]
		public async Task TestRegisterUserAsync_UserWithExistingEmail_ReturnsFailed()
		{
			// Arrange
			var firstResult = await AuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");
			Assert.IsTrue(firstResult.Succeeded, "Precondition: First registration should succeed");

			// Act
			var secondResult = await AuthenticationService.RegisterUserAsync("AnotherUser", "testuser@hotmail.com", "someweirdpassword123");

			// Assert
			Assert.IsFalse(secondResult.Succeeded, "Registration should fail due to duplicate email.");
			Assert.IsNotNull(secondResult.Error);
			await AssertAmountOfUsersSaved(1);
		}

		[Test]
		public async Task TestLoginAsync_WithValidCredentials_Succeeds()
		{
			// Arrange
			await AuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");

			// Act
			var result = await AuthenticationService.LoginAsync("testuser@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsTrue(result.Succeeded, "Login with valid credentials should succeed.");
			Assert.IsNotNull(result.Token, "A JWT token should be returned on successful login.");
			Assert.IsNotNull(result.User, "User details should be returned on successful login.");
			Assert.AreEqual("TestUser", result.User!.Username);
			Assert.AreEqual("testuser@hotmail.com", result.User.Email);
			Assert.IsNull(result.Error);
		}

		[Test]
		public async Task TestLoginAsync_WithWrongPassword_ReturnsFailed()
		{
			// Arrange
			await AuthenticationService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");

			// Act
			var result = await AuthenticationService.LoginAsync("testuser@hotmail.com", "wrongpassword");

			// Assert
			Assert.IsFalse(result.Succeeded, "Login with wrong password should fail.");
			Assert.IsNull(result.Token);
			Assert.IsNull(result.User);
			Assert.IsNotNull(result.Error);
		}

		[Test]
		public async Task TestLoginAsync_WithNonExistentEmail_ReturnsFailed()
		{
			// Act
			var result = await AuthenticationService.LoginAsync("nobody@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsFalse(result.Succeeded, "Login with a non-existent email should fail.");
			Assert.IsNull(result.Token);
			Assert.IsNull(result.User);
			Assert.IsNotNull(result.Error);
		}

		[Test]
		public async Task TestLoginAsync_UserServiceFailure_ReturnsFailed()
		{
			// Arrange
			var mockUserService = new Mock<IUserService>();
			mockUserService
				.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(Result<User>.Failure(new[] { "Service error" }));

			var failingAuthenticationService = new AuthenticationService(Factory, _provider.GetRequiredService<ILogger<AuthenticationService>>(), mockUserService.Object, _provider.GetRequiredService<IJwtService>());

			// Act
			var result = await failingAuthenticationService.LoginAsync("testuser@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsFalse(result.Succeeded);
			Assert.IsNotNull(result.Error);
		}
	}
}
