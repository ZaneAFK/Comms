using Comms_Server.Database.Models.User;
using Comms_Server.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Services.Authentication
{
	[TestFixture]
	public class AuthManagerTest
	{
		[Test]
		public async Task TestCreateAsync_ReturnsSuccess_WhenUserManagerSucceeds()
		{
			// Arrange
			var mockUserManager = SetupUserManagerMock();

			mockUserManager
				.Setup(um => um.CreateAsync(It.IsAny<SecurityUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Success);

			var authManager = new AuthManager(mockUserManager.Object);
			var user = new SecurityUser { UserName = "alice", Email = "alice@example.com" };
			const string password = "Password123!";

			// Act
			var result = await authManager.CreateAsync(user, password);

			// Assert
			Assert.True(result.Succeeded);
			mockUserManager.Verify(um => um.CreateAsync(It.Is<SecurityUser>(u => u.UserName == "alice"), password), Times.Once);
		}

		[Test]
		public async Task TestCreateAsync_ReturnsFailure_WhenUserManagerFails()
		{
			// Arrange
			var mockUserManager = SetupUserManagerMock();

			var identityError = new IdentityError { Description = "Invalid password" };
			mockUserManager
				.Setup(um => um.CreateAsync(It.IsAny<SecurityUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Failed(identityError));

			var authManager = new AuthManager(mockUserManager.Object);
			var user = new SecurityUser { UserName = "alice", Email = "alice@example.com" };
			const string password = "bad";

			// Act
			var result = await authManager.CreateAsync(user, password);

			// Assert
			Assert.False(result.Succeeded);
			Assert.IsNotEmpty(result.Errors);
			Assert.IsTrue(result.Errors.Any(e => e.Description == "Invalid password"));
			mockUserManager.Verify(um => um.CreateAsync(It.Is<SecurityUser>(u => u.UserName == "alice"), password), Times.Once);
		}

		Mock<UserManager<SecurityUser>> SetupUserManagerMock()
		{
			var userStoreMock = new Mock<IUserStore<SecurityUser>>();

			return new Mock<UserManager<SecurityUser>>(
				userStoreMock.Object,
				null, // IOptions<IdentityOptions>
				null, // IPasswordHasher<TUser>
				null, // IEnumerable<IUserValidator<TUser>>
				null, // IEnumerable<IPasswordValidator<TUser>>
				null, // ILookupNormalizer
				null, // IdentityErrorDescriber
				null, // IServiceProvider
				null  // ILogger<UserManager<TUser>>
			);
		}
	}
}
