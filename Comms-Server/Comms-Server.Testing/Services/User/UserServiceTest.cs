using Comms_Server.Services.User;
using Comms_Server.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Comms_Server.Testing.Services.User
{
	[TestFixture]
	public class UserServiceTest : TransactionalTest
	{
		public required UserService UserService;

		[SetUp]
		public override async Task Setup()
		{
			await base.Setup();

			UserService = (UserService)_provider.GetRequiredService<IUserService>();
		}

		[Test]
		public async Task RegisterUserAsync_WithValidInput_ReturnsSuccess()
		{
			// Act
			var result = await UserService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");

			// Assert
			Assert.IsTrue(result.Succeeded, "Registration with valid input should succeed.");
			Assert.IsNotNull(result.Value, "Result value should not be null on success.");
			Assert.AreEqual("TestUser", result.Value!.UserName, "Registered user should have the correct username.");
			Assert.AreEqual("testuser@hotmail.com", result.Value.Email, "Registered user should have the correct email.");
			await AssertAmountOfUsersSaved(1);
		}

		[Test]
		public async Task RegisterUserAsync_WithDuplicateEmail_ReturnsFailure()
		{
			// Arrange
			var firstResult = await UserService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");
			Assert.IsTrue(firstResult.Succeeded, "Precondition: first registration should succeed.");

			// Act
			var secondResult = await UserService.RegisterUserAsync("AnotherUser", "testuser@hotmail.com", "anotherpassword123!");

			// Assert
			Assert.IsFalse(secondResult.Succeeded, "Registration with a duplicate email should fail.");
			await AssertAmountOfUsersSaved(1);
		}

		[Test]
		public async Task GetByIdAsync_WithExistingUser_ReturnsUser()
		{
			// Arrange
			var registrationResult = await UserService.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!");
			Assert.IsTrue(registrationResult.Succeeded, "Precondition: registration should succeed.");
			var registeredUser = registrationResult.Value!;

			// Act
			var foundUser = await UserService.GetByIdAsync(registeredUser.Id);

			// Assert
			Assert.IsNotNull(foundUser, "GetByIdAsync should return the user when they exist.");
			Assert.AreEqual(registeredUser.Id, foundUser!.Id, "Returned user should have the same ID as the registered user.");
			Assert.AreEqual(registeredUser.UserName, foundUser.UserName, "Returned user should have the correct username.");
			Assert.AreEqual(registeredUser.Email, foundUser.Email, "Returned user should have the correct email.");
		}

		[Test]
		public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
		{
			// Act
			var foundUser = await UserService.GetByIdAsync(Guid.NewGuid());

			// Assert
			Assert.IsNull(foundUser, "GetByIdAsync should return null when the user does not exist.");
		}
	}
}
