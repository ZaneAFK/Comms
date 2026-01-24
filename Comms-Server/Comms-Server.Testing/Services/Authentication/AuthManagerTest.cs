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

			mockUserManager
				.Setup(um => um.AddToRolesAsync(It.IsAny<SecurityUser>(), It.IsAny<IEnumerable<string>>()))
				.ReturnsAsync(IdentityResult.Success);

			var authManager = new AuthManager(mockUserManager.Object);
			var user = new SecurityUser { UserName = "alice", Email = "alice@example.com" };
			const string password = "Password123!";

			// Act
			var result = await authManager.RegisterSecurityUser(user, password);

			// Assert
			Assert.True(result.Succeeded);
			mockUserManager.Verify(um => um.CreateAsync(It.Is<SecurityUser>(u => u.UserName == "alice"), password), Times.Once);
			mockUserManager.Verify(um => um.AddToRolesAsync(
				It.Is<SecurityUser>(u => u.UserName == "alice"),
				It.Is<IEnumerable<string>>(roles => roles != null && roles.SequenceEqual(new[] { "User" }))
			), Times.Once);
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

			// Ensure AddToRolesAsync is stubbed to avoid uninitialized Task on call
			mockUserManager
				.Setup(um => um.AddToRolesAsync(It.IsAny<SecurityUser>(), It.IsAny<IEnumerable<string>>()))
				.ReturnsAsync(IdentityResult.Success);

			var authManager = new AuthManager(mockUserManager.Object);
			var user = new SecurityUser { UserName = "alice", Email = "alice@example.com" };
			const string password = "bad";

			// Act
			var result = await authManager.RegisterSecurityUser(user, password);

			// Assert
			Assert.False(result.Succeeded);
			Assert.IsNotEmpty(result.Errors);
			Assert.IsTrue(result.Errors.Any(e => e.Description == "Invalid password"));
			mockUserManager.Verify(um => um.CreateAsync(It.Is<SecurityUser>(u => u.UserName == "alice"), password), Times.Once);
			mockUserManager.Verify(um => um.AddToRolesAsync(
				It.Is<SecurityUser>(u => u.UserName == "alice"),
				It.Is<IEnumerable<string>>(roles => roles != null && roles.SequenceEqual(new[] { "User" }))
			), Times.Once);
		}

		[Test]
		public void RegisterSecurityUser_ThrowsWrappedException_WhenUserManagerThrows()
		{
			// Arrange
			var mockUserManager = SetupUserManagerMock();

			// Simulate an unexpected exception from UserManager.CreateAsync
			mockUserManager
				.Setup(um => um.CreateAsync(It.IsAny<SecurityUser>(), It.IsAny<string>()))
				.ThrowsAsync(new InvalidOperationException("boom"));

			var authManager = new AuthManager(mockUserManager.Object);
			var user = new SecurityUser { UserName = "alice", Email = "alice@example.com" };
			const string password = "Password123!";

			// Act & Assert - ensure the exception is wrapped with the expected message
			var ex = Assert.ThrowsAsync<Exception>(async () => await authManager.RegisterSecurityUser(user, password));
			Assert.IsNotNull(ex);
			Assert.That(ex.Message, Is.EqualTo("Error registering user: boom"));

			// Ensure roles were not attempted to be added after the exception
			mockUserManager.Verify(um => um.AddToRolesAsync(It.IsAny<SecurityUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
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
